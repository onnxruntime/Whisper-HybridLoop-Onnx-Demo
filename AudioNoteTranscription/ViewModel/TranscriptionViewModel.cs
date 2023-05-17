using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioNoteTranscription.Common;
using AudioNoteTranscription.Model;

namespace AudioNoteTranscription.ViewModel
{
    public class TranscriptionViewModel : ModelBase
    {
        public TranscriptionViewModel(TranscriptionModel model)
        {
            _model = model;
            _noteName = "Placeholder.txt";
            _transcription = "Please select an audio file to transcribe.";
            _audioFileName = String.Empty;

            _saveCommand = new CommandBase<object>(
                async (obj) => await _model.StoreTranascription(_noteName, _transcription), (obj) => true);
            _transcribeCommand = new CommandBase<object>(
                async (obj) => {
                    Transcription = String.Empty;
                    Transcription = await _model.TranscribeAsync(_audioFileName, _useCloudInference);
                }, (obj) => true);
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

        private bool _useCloudInference;
        public bool UseCloudInference
        {
            get => _useCloudInference;
            set => SetProperty(ref _useCloudInference, value);
        }

        private TranscriptionModel _model;

        public TranscriptionModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
    }
}
