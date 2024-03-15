
using AudioNoteTranscription.Model;

namespace AudioNoteTranscription.ViewModel;

public class ViewModelLocator
{
    private static ViewModelLocator instance = new ViewModelLocator();
    public static ViewModelLocator Instance { get { return instance; } }
    public static TranscriptionViewModel TranscriptionViewModel { get; } = new TranscriptionViewModel(new TranscriptionModel());

}
