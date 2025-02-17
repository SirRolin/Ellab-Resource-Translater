using Ellab_Resource_Translater.Objects;
using System.Collections;
using System.Resources;

namespace Ellab_Resource_Translater.Util
{
    public class ResourceHandler
    {
        /// <summary>
        /// Reads the resource file (.resx) and returns a dictionary of the entries with the key as the dictionary key.
        /// </summary>
        /// <typeparam name="Type">Type of value, if you plan to write back to the resource file, this should be <see cref="object"/>?, otherwise it filters to only the correct types.</typeparam>
        /// <param name="path">path of the resource</param>
        /// <returns>Dictionary with key, <see cref="MetaData"/>, which is a (key, value, comment) object, that can implicitly be converted to a <see cref="ResXDataNode"/>.</returns>
        public static Dictionary<string, MetaData<Type>> ReadResource<Type>(string path)
        {
            Dictionary<string, MetaData<Type>> trans = [];
            using (ResXResourceReader resxReader = new(path))
            {
                using ResXResourceReader resxCommentReader = new(path);
                // Switches to reading metaData instead of values, can't have both, which we need for comments
                resxCommentReader.UseResXDataNodes = true;

                // Found out that some files are simply broken which will cause this to throw an error when it reaches the end of the file.
                try
                {
                    var enumerator = resxCommentReader.GetEnumerator();
                    foreach (DictionaryEntry entry in resxReader)
                    {
                        string key = entry.Key.ToString() ?? string.Empty;
                        string comment;

                        // Since we have 2 readers of the same File, we can iterate over them synced by calling MoveNext only once per loop
                        if (enumerator.MoveNext())
                        {
                            ResXDataNode? current = (ResXDataNode?)((DictionaryEntry)enumerator.Current).Value;
                            comment = current?.Comment ?? string.Empty;
                        }
                        else
                            comment = string.Empty;

                        if (entry.Value is Type value)
                            trans.Add(key, new MetaData<Type>(key, value, comment));
                    }
                }
                catch
                {
                }
            }
            return trans;
        }

        /// <summary>
        /// Writes the <paramref name="data"/> into <paramref name="path"/> resource file.
        /// </summary>
        /// <remarks>
        /// this might throw an IO error if path is incorrect or access is blocked.
        /// </remarks>
        /// <param name="path">File Path.</param>
        /// <param name="data"></param>
        public static void WriteResource(string path, Dictionary<string, MetaData<object?>> data)
        {
            using ResXResourceWriter resxWriter = new(path);
            foreach (var entry in data)
            {
                resxWriter.AddResource(entry.Value);
            }
        }
    }
}
