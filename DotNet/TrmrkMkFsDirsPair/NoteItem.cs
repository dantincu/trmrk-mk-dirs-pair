using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrmrkMkFsDirsPair
{
    /// <summary>
    /// DTO containing the note title that is serialized to json and written into the .keep file residing
    /// in the full name folder.
    /// </summary>
    public class NoteItem
    {
        /// <summary>
        /// Gets or sets the note title.
        /// </summary>
        public string Title { get; set; }
    }
}
