using System;
using System.IO;

namespace Stashify.Validation
{
    public class FileValidator : IFileValidator
    {
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            var files = new string[] { Configuration.ASSET_LIST_FILE, Configuration.COLOR_INDEX_FILE };

            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError($"{file} not found."));
                }
            }


            if (!Directory.Exists(Configuration.ASSET_DIR))
            {
                try
                {
                    Directory.CreateDirectory(Configuration.ASSET_DIR);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError($"Couldn't create asset directory. {ex.Message}"));
                }
            }

            return result;
        }
    }
}
