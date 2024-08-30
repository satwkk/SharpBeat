using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using SharpBeat.AudioExtractors;
using SharpBeat.Models;
using SharpBeat.Utility;
using System.Collections.Concurrent;

namespace SharpBeat.Services 
{

    public enum EAudioClientState 
    {
        IDLE, PLAYING, EXTRACTING
    }

    public class QueueItem 
    {
        public IAudioClient AudioClient { get; set; }
        public IVoiceChannel VoiceChannel { get; set; }
        public ulong VoiceChannelId { get; set; }
        public Queue<IPlayable> ExtractedTracks { get; set; }
        public IMessageChannel CommandInvokedChannel { get; set; }
        public EAudioClientState State { get; set; }
        public CancellationTokenSource WriteTokenSource { get; set; }
    }

    public class MusicService 
    {

        private readonly ConcurrentDictionary<ulong, QueueItem> _queue = new();

        private readonly DiscordSocketClient _discordClient;

        // TODO: Can we remove this hard reference to a concrete class ??
        private readonly MusicWatcherService _watcher;

        private readonly MessagingService _messagingService;

        public MusicService(DiscordSocketClient client, MusicWatcherService watcher, MessagingService messagingService) 
        {
            _discordClient = client;
            _watcher = watcher;
            _messagingService = messagingService;
            _watcher.ProcessQueueEvent += ProcessQueueCallback;
            _ = Task.Run(async () => await _watcher.Watch(_queue));
        }

        /// <summary>
        /// Processes the queue for the channel. This is invoked by the Watcher service which checks
        /// if the audio client is idle and there are songs to process. If so is the case it fires an event to process the queue of specific channel.
        /// </summary>
        /// <param name="guildId"> The id of channel whose queue will be processed. </param>
        async void ProcessQueueCallback(ulong guildId) 
        {
            var channelQueueItem = await TryGetChannelItemAsync(guildId);
            if (channelQueueItem == null) return;

            // NOTE: IMPORTANT, ELSE THE WATCHER WILL KEEP FIRING EVENT TO THIS AUDIOCLIENT SINCE IT IS NOT IN PLAYING STATE !!!
            channelQueueItem.State = EAudioClientState.EXTRACTING;

            // Get the first track from the queue
            var track = channelQueueItem.ExtractedTracks.Dequeue();

            // Convert the audio to PCM format
            var audioConverter = new AudioConverter();
            var pcmStream = await audioConverter.ConvertToPCM(track.Stream);

            // Stream the PCM stream into discord audio client.
            using (var discordStream = channelQueueItem.AudioClient.CreatePCMStream(AudioApplication.Music)) 
            {
                try
                {
                    // Set the state to playing to avoid callbacks from the watcher
                    channelQueueItem.State = EAudioClientState.PLAYING;
                    channelQueueItem.WriteTokenSource = new CancellationTokenSource();

                    // Set the embed message and write the audio stream to the discord's standard output
                    await _messagingService.SendMessage(channelQueueItem.CommandInvokedChannel, track);
                    await discordStream.WriteAsync(pcmStream, channelQueueItem.WriteTokenSource.Token);

                }
                
                // Catch exception if the song is skipped which stops the asynchronous write operation above to the discord stream.
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine("Operation was cancelled. No issue. Go next !!");
                }

                // Finally we flush the standard out and set the state back to idle to resume watcher to process this channel's queue.
                finally
                {
                    await discordStream.FlushAsync();
                    channelQueueItem.State = EAudioClientState.IDLE;
                }

                await audioConverter.Handle.WaitForExitAsync();
            }
        }

        /// <summary>
        /// Joins the voice channel of user who invoked the play command.
        /// </summary>
        /// <param name="voiceChannel"> The voice channel to join to </param>
        /// <param name="channel"> The message channel where the command was invoked </param>
        /// <returns></returns>
        public async Task JoinChannel(IVoiceChannel voiceChannel, IMessageChannel channel) 
        {
            // If the invoked user is not in any voice channel
            if (voiceChannel == null) 
            {
                await channel.SendMessageAsync("User needs to in a voice channel.");
                return;
            }

            // If the bot is already is a voice channel
            if (_queue.ContainsKey(voiceChannel.Guild.Id)) 
            {
                Console.WriteLine("[DEBUG]: Already in a voice channel. Not connecting again");
                return;
            }

            // Connect to the voice channel if everything above passes
            await voiceChannel.ConnectAsync();
        }

        /// <summary>
        /// Checks if the user who invoked the music command is in any voice channel
        /// </summary>
        /// <param name="user"> The user who invoked the command </param>
        /// <returns></returns>
        public async Task<bool> IsInAnyVoiceChannel(IGuildUser user) 
        {
            var vc = user.VoiceChannel;
            return await Task.FromResult(vc != null);
        }

        /// <summary>
        /// Queues a song request to be played. This is processed by the MusicWatcherService later.
        /// </summary>
        /// <param name="context"> Discord Context object </param>
        /// <param name="input"> The input requested by the user </param>
        /// <returns></returns>
        public async Task Queue(SocketCommandContext context, string input) 
        {
            if (!_queue.ContainsKey(context.Guild.Id))
            {
                // Initialize the queue item structure when bot joins a voice channel.
                _queue[context.Guild.Id] = new QueueItem() 
                {
                    AudioClient = context.Guild.AudioClient,
                    State = EAudioClientState.IDLE,
                    VoiceChannel = (context.Message.Author as IGuildUser)!.VoiceChannel,
                    ExtractedTracks = new Queue<IPlayable>(),
                    CommandInvokedChannel = context.Message.Channel,
                    VoiceChannelId = context.Guild.Id
                };
            }

            // Extract the tracks
            _ = Task.Run( async () => 
            {
                try 
                {
                    var result = await ExtractorFactory.GetExtractor( input ).Extract( input );
                    (result as IQueueItem<IPlayable>)?.AddToQueue(_queue[context.Guild.Id].ExtractedTracks);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR]: " + e.ToString());
                }
            } );

            await _messagingService.SendMessage(context.Message.Channel, context.Message.Author as IGuildUser, input);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Skips the currently playing song. 
        /// </summary>
        /// <param name="context"> Discord context </param>
        /// <returns></returns>
        public async Task SkipAsync(SocketCommandContext context) 
        {
            var queueItem = await TryGetChannelItemAsync(context.Guild.Id);
            await queueItem.WriteTokenSource.CancelAsync();
        }

        /// <summary>
        /// Gets the queue item for a specific channel.
        /// </summary>
        /// <param name="id"> The id of channel </param>
        /// <returns></returns>
        public async Task<QueueItem> TryGetChannelItemAsync(ulong id) 
        {
            if (_queue.TryGetValue(id, out QueueItem item)) 
            {
                return await Task.FromResult(item);
            }

            throw new KeyNotFoundException("Trying to get a queue item when bot is not in that channel");
        }

        /// <summary>
        /// Leaves the voice channel and clears the queue items related to the channel.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LeaveAsync(ulong id) 
        {
            var queueItem = await TryGetChannelItemAsync( id );
            await queueItem.VoiceChannel.DisconnectAsync();
            _queue.Remove(id, out QueueItem? item);
        }

        /// <summary>
        /// Sends back all the queued songs in an embed
        /// </summary>
        /// <param name="context"> The discord command context </param>
        /// <returns></returns>
        public async Task ListQueue(SocketCommandContext context)
        {
            var queueItem = await TryGetChannelItemAsync(context.Guild.Id);

            var idx = 0;
            string embedString = string.Empty;

            queueItem.ExtractedTracks.ToList().ForEach(track => 
            {
                embedString += $"{++idx}. {track.Name}\n";
            });

            var queueEmbed = new EmbedBuilder()
            .WithTitle("Queued Songs...")
            .AddField(embedString, '\u200B')
            .Build();

            await _messagingService.SendMessage(context.Message.Channel, queueEmbed);
        }
    }
}