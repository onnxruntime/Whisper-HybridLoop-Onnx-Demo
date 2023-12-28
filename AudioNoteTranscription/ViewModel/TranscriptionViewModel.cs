using System;
using System.IO;
using System.Linq;
using AudioNoteTranscription.Common;
using AudioNoteTranscription.Extensions;
using AudioNoteTranscription.Model;
using AudioNoteTranscription.Whisper;

namespace AudioNoteTranscription.ViewModel
{
    public class TranscriptionViewModel : ModelBase
    {
        bool redy = true;
        public TranscriptionViewModel(TranscriptionModel model)
        {
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MlModels");
            if (Directory.Exists(modelPath))
            {
                _modelPath = Directory.GetDirectories(modelPath).FirstOrDefault();
            }

            _model = model;
            _model.MessageRecognized += _model_MessageRecognized;

            _noteName = "Placeholder.txt";
            _transcription = "Please select an audio file to transcribe.";
            _audioFileName = String.Empty;

            _saveCommand = new CommandBase<object>(
                async (obj) => await _model.StoreTranascriptionAsync(_noteName, _transcription), (obj) => true);
            _transcribeCommand = new CommandBase<object>(
                async (obj) =>
                {
                    redy = false;
                    Transcription = String.Empty;

                    var config = new WhisperConfig(_modelPath, _audioFileName, _selectedLanguage, selectedProvider);

                    await _model.TranscribeAsync(config, IsRealtime, IsMic, IsLoopBack);

                    redy = true;
                }, (obj) => redy);

            _stopCommand = new CommandBase<object>(async (obj) => await _model.StopRecognitionAsync(), (obj) => true);

            _loopbackCommand = new CommandBase<object>((state) =>
            {
                if (state != null && Convert.ToBoolean(state) == true)
                {
                    model.StartLoopback();
                }
                else
                {
                    model.StopLoopback();
                }
            }, (obj) => true);

            _micCommand = new CommandBase<object>((state) =>
            {
                if (state != null && Convert.ToBoolean(state) == true)
                {
                    model.StartMic();

                }
                else
                {
                    model.StopMic();
                }
            }, (obj) => true);

            _clearTextCommand = new CommandBase<object>((state) =>
            {
                _model.ClearText();
            }, (obj) => true);
        }

        private void _model_MessageRecognized(object? sender, EventArgs e)
        {
            Transcription = (e as MessageRecognizedEventArgs)?.RecognizedText ?? string.Empty;
        }

        private CommandBase<object> _transcribeCommand;

        public CommandBase<object> TranscribeCommand
        {
            get => _transcribeCommand;
            set => SetProperty(ref _transcribeCommand, value);
        }

        private CommandBase<object> _saveCommand;

        public CommandBase<object> SaveCommand
        {
            get => _saveCommand;
            set => SetProperty(ref _saveCommand, value);
        }

        private CommandBase<object> _stopCommand;

        public CommandBase<object> StopCommand
        {
            get => _stopCommand;
            set => SetProperty(ref _stopCommand, value);
        }

        private string _transcription;

        public string Transcription
        {
            get => _transcription;
            set => SetProperty(ref _transcription, value);
        }

        private string _noteName;

        public string NoteName
        {
            get => _noteName;
            set => SetProperty(ref _noteName, value);
        }

        private string _audioFileName;

        public string AudioFileName
        {
            get => _audioFileName;
            set => SetProperty(ref _audioFileName, value);
        }

        private TranscriptionModel _model;

        public TranscriptionModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private string _selectedLanguage = "en";

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public string[] Languages => Inference.ALL_LANGUAGE_TOKENS.Keys.ToArray();


        private string _modelPath;
        public string ModelPath
        {
            get => _modelPath;
            set => SetProperty(ref _modelPath, value);
        }

        private bool _isRealtime = true;

        public bool IsRealtime
        {
            get => _isRealtime;
            set => SetProperty(ref _isRealtime, value);
        }

        private bool _isMic;

        public bool IsMic
        {
            get => _isMic;
            set => SetProperty(ref _isMic, value);
        }

        private bool _isLoopback = true;

        public bool IsLoopBack
        {
            get => _isLoopback;
            set => SetProperty(ref _isLoopback, value);
        }

        private ExecutionProvider selectedProvider = ExecutionProvider.Cpu;

        public ExecutionProvider SelectedProvider { get => selectedProvider; set => SetProperty(ref selectedProvider, value); }

        private CommandBase<object> _micCommand;

        public CommandBase<object> MicCommand
        {
            get => _micCommand;
            set => SetProperty(ref _micCommand, value);
        }

        private CommandBase<object> _loopbackCommand;

        public CommandBase<object> LoopbackCommand
        {
            get => _loopbackCommand;
            set => SetProperty(ref _loopbackCommand, value);
        }

        private CommandBase<object> _clearTextCommand;

        public CommandBase<object> ClearTextCommand
        {
            get => _clearTextCommand;
            set => SetProperty(ref _clearTextCommand, value);
        }
    }
}
