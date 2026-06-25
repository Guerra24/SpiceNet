namespace SpiceNet.UWP.Util;

/*
   https://github.com/FabioJe/INIParser
   
   Copyright

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

public sealed class IniFile
{
    internal readonly Dictionary<string, Dictionary<string, string?>> Data = new();
    private string[]? sections;

    public IniFile() { }

    public async Task LoadFileAsync(StorageFile file)
    {
        var strings = await FileIO.ReadLinesAsync(file);
        ParseText(strings);
    }

    private void ParseText(IEnumerable<string> strings)
    {
        Data.Clear();
        sections = null;
        Dictionary<string, string?>? lastSec = null; ;
        foreach (var line in strings)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var pureText = line;
            if (char.IsWhiteSpace(pureText[0]))
                pureText = pureText.Trim();
            var firstChar = pureText[0];
            if (firstChar == ';' || firstChar == '#') continue;
            if (firstChar == '[' && pureText[^1] == ']')
            {
                var lastBlock = pureText[1..^1];
                if (!Data.ContainsKey(lastBlock))
                {
                    lastSec = new Dictionary<string, string?>();
                    Data.Add(lastBlock, lastSec);
                }
                else
                    lastSec = Data[lastBlock];
                continue;
            }
            if (lastSec is null) continue;
            var data = pureText.Split('=', 2);
            if (data.Length != 2) { continue; }
            var key = Trim(data[0]);
            lastSec[key] = Trim(data[1]).Replace(@"\n", "\n");
        }

    }

    private static string Trim(string t)
    {
        if (t.Length == 0) return t;
        if (char.IsWhiteSpace(t[0]) || char.IsWhiteSpace(t[^1]))
            return t.Trim();
        return t;
    }

    public string? this[string section, string name]
    {
        get
        {
            if (Data.TryGetValue(section, out var dsection))
                if (dsection.TryGetValue(name, out var text))
                    return text;
            return null;
        }
        set
        {
            if (Data.TryGetValue(section, out var dsection))
            {
                dsection[name] = value;
            }
            else
            {
                var keyValues = new Dictionary<string, string?>
                    {
                        { name, value }
                    };
                Data.Add(section, keyValues);
            }
        }
    }

    public string this[string section, string name, string defaultValue]
    {
        get
        {
            var result = this[section, name];
            if (string.IsNullOrEmpty(result)) return defaultValue;
            return result;
        }
    }

    public string[] Sections
    {
        get
        {
            sections ??= [.. Data.Keys];
            return sections;

        }
    }

    public string[] GetKeys(string section)
    {
        if (Data.TryGetValue(section, out var dsection))
            return [.. dsection.Keys];
        return [];
    }

    public void RemoveSection(string section)
    {
        Data.Remove(section);
    }

    public void RemoveValue(string section, string key)
    {
        if (Data.TryGetValue(section, out var dsection))
            dsection.Remove(key);
    }

    public void AddSection(string name)
    {
        if (!Data.ContainsKey(name))
            Data.Add(name, []);
    }

}
