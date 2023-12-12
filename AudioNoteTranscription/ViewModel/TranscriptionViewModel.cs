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
            _modelPath = Directory.GetDirectories(Path.Combine(Directory.GetCurrentDirectory(), "MlModels")).FirstOrDefault();
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

                    Transcription = await _model.TranscribeAsync(_audioFileName, _selectedLanguage, _modelPath, IsRealtime, selectedProvider);


                    redy = true;
                }, (obj) => redy);

            _stopCommand = new CommandBase<object>(async (obj) =>await _model.StopReognitionAsync(), (obj) => true);
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

        private bool _isRealtime;

        public bool IsRealtime
        {
            get => _isRealtime;
            set => SetProperty(ref _isRealtime, value);
        }

        private ExecutionProvider selectedProvider = ExecutionProvider.Cpu;

        public ExecutionProvider SelectedProvider { get => selectedProvider; set => SetProperty(ref selectedProvider, value); } 
    }
}
