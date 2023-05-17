
using AudioNoteTranscription.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioNoteTranscription.ViewModel
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        public static TranscriptionViewModel TranscriptionViewModel { get; } = new TranscriptionViewModel(new TranscriptionModel());

    }
}
