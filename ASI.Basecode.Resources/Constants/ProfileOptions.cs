namespace ASI.Basecode.Resources.Constants
{
    /// <summary>Shared program choices and profile field options.</summary>
    public static class ProfileOptions
    {
        /// <summary>The organization's current program list; add a program here when it changes.</summary>
        public static readonly IReadOnlyList<string> Courses = new[] { "BSCS", "BSIT" };

        // Recognize legacy long labels without rewriting historical registration snapshots.
        public static string? NormalizeCourse(string? course) => course?.Trim().ToUpperInvariant() switch
        {
            "BSIT" or "BS INFORMATION TECHNOLOGY" => "BSIT",
            "BSCS" or "BS COMPUTER SCIENCE" => "BSCS",
            _ => course?.Trim()
        };

        /// <summary>Year levels offered by the form.</summary>
        public static readonly IReadOnlyList<int> YearLevels = new[] { 1, 2, 3, 4 };

        /// <summary>Sections offered by the form.</summary>
        public static readonly IReadOnlyList<string> Sections = new[] { "A", "B", "C", "D", "E", "F" };

        /// <summary>Student numbers are exactly eight ASCII digits starting with 2; the value stays a string so the column keeps its shape.</summary>
        public const string StudentNumberPattern = "^2[0-9]{7}$";

        /// <summary>Mobile numbers are optional, but a supplied number is exactly eleven ASCII digits.</summary>
        public const string MobileNumberPattern = "^09[0-9]{9}$";

        /// <summary>A section is one letter from the offered set; the service upper-cases what it stores.</summary>
        public const string SectionPattern = "^[A-Fa-f]$";

        /// <summary>A password is at least eight characters and carries a letter and a digit.</summary>
        public const string PasswordPattern = "^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$";

        // Trim and upper-case a section without rewriting anything else; blank collapses to null.
        public static string? NormalizeSection(string? section) =>
            string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToUpperInvariant();

        // Title-case a name without changing its structure: initials, hyphens and apostrophes survive.
        public static string? NormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var collapsed = string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            var characters = collapsed.ToCharArray();
            var startOfWord = true;

            for (var index = 0; index < characters.Length; index++)
            {
                var character = characters[index];
                if (char.IsWhiteSpace(character) || character == '-' || character == '\'' || character == '.')
                {
                    startOfWord = true;
                    continue;
                }

                characters[index] = startOfWord ? char.ToUpperInvariant(character) : char.ToLowerInvariant(character);
                startOfWord = false;
            }

            return new string(characters);
        }

        /// <summary>Membership types the organization uses.</summary>
        public static readonly IReadOnlyList<string> MembershipTypes = new[] { "Regular", "Officer" };
    }
}
