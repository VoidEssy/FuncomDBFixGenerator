using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FuncomDBFixGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Specify the file name (assuming it is in the same directory as the executable)
            string fileName = "ConanSandbox.log";
            string outputFileName = "GeneratedSql.sql";

            // Define the regex pattern (for example, capturing any content between specific tags)
            string pattern = @"\[\d+.\d+.\d+-\d+.\d+.\d+.\d+\].* NameToLoad: (.*)\n.*\n\[\d+.\d+.\d+-\d+.\d+.\d+.\d+\].* String asset reference \""None\"".*slow.";

            try
            {
                // Check if the file exists in the same directory as the executable
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File '{fileName}' not found in the current directory.");
                    return;
                }

                // Read entire content of the file
                string content = File.ReadAllText(filePath);

                Console.WriteLine($"Searching for pattern '{pattern}' in '{fileName}'...");

                // Use RegexOptions.Singleline to allow '.' to match newline characters
                MatchCollection matches = Regex.Matches(content, pattern);

                // Write matches to the output file
                string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

                if (File.Exists(outputFilePath))
                {
                    Console.WriteLine($"File '{outputFileName}' already exists and will be deleted.");
                    File.Delete(outputFilePath);
                }

                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (Match match in matches)
                    {
                        string target = match.Groups[1].ToString().Replace("\r", "");
                        writer.WriteLine($"DELETE FROM buildable_health WHERE object_id IN(SELECT DISTINCT object_id FROM buildings WHERE object_id IN (SELECT DISTINCT object_id FROM properties WHERE object_id IN (SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%'))));");
                        writer.WriteLine($"DELETE FROM buildings WHERE object_id IN(SELECT DISTINCT object_id FROM properties WHERE object_id IN (SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%')));");
                        writer.WriteLine($"DELETE FROM properties WHERE object_id IN(SELECT id FROM (SELECT id, trim(substr(class, INSTR(class, '/BP'), length(class)), '/') AS name FROM actor_position WHERE class LIKE '{target}%'));");
                        writer.WriteLine($"DELETE FROM actor_position WHERE class LIKE '{target}%';");
                        writer.WriteLine("--");  // Separator between matches
                    }
                    writer.WriteLine("VACUUM;");
                    writer.WriteLine("REINDEX;");
                    writer.WriteLine("ANALYZE;");
                    writer.WriteLine("PRAGMA integrity_check;");
                }

                Console.WriteLine($"Matches used to generate script in file: '{outputFileName}' with '--' as a separator.");
                Console.WriteLine($"Hit Any Key to Close.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
