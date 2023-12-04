
using AudioNoteTranscription.Model;

namespace AudioNoteTranscription.ViewModel
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        public static TranscriptionViewModel TranscriptionViewModel { get; } = new TranscriptionViewModel(new TranscriptionModel());

    }
}
