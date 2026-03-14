// Copyright (c) 2025 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cubendo_Remote_Panel
{
    public static class AhkKeyMapper
    {
        /// <summary>
        /// Converts a user-friendly key string (e.g. "Ctrl+Alt+Numpad1") to an AutoHotkey key sequence.
        /// Supports quoted strings: e.g. Ctrl+Alt+"Hello World"+X
        /// </summary>
        public static string ToAhkKey(string keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString))
                return "";

            List<string> parts = SplitKeyStringRespectingQuotes(keyString);
            string ahkKey = "";
            foreach (string part in parts)
            {
                string p = part.Trim();
                // Handle quoted string as literal
                if (p.StartsWith("\"") && p.EndsWith("\"") && p.Length >= 2)
                {
                    ahkKey += p.Substring(1, p.Length - 2);
                    continue;
                }
                switch (p.ToLowerInvariant())
                {
                    // Modifier keys
                    case "ctrl": ahkKey += "^"; break;
                    case "alt": ahkKey += "!"; break;
                    case "shift": ahkKey += "+"; break;
                    case "win": ahkKey += "#"; break;

                    // Navigation and control keys
                    case "tab": ahkKey += "{Tab}"; break;
                    case "enter": ahkKey += "{Enter}"; break;
                    case "esc":
                    case "escape": ahkKey += "{Esc}"; break;
                    case "space": ahkKey += "{Space}"; break;
                    case "backspace": ahkKey += "{Backspace}"; break;
                    case "delete":
                    case "del": ahkKey += "{Delete}"; break;
                    case "insert": ahkKey += "{Insert}"; break;
                    case "home": ahkKey += "{Home}"; break;
                    case "end": ahkKey += "{End}"; break;
                    case "pgup":
                    case "pageup": ahkKey += "{PgUp}"; break;
                    case "pgdn":
                    case "pagedown": ahkKey += "{PgDn}"; break;
                    case "up":
                    case "uparrow": ahkKey += "{Up}"; break;
                    case "down":
                    case "downarrow": ahkKey += "{Down}"; break;
                    case "left":
                    case "leftarrow": ahkKey += "{Left}"; break;
                    case "right":
                    case "rightarrow": ahkKey += "{Right}"; break;

                    // Numpad keys
                    case "numpad0": ahkKey += "{Numpad0}"; break;
                    case "numpad1": ahkKey += "{Numpad1}"; break;
                    case "numpad2": ahkKey += "{Numpad2}"; break;
                    case "numpad3": ahkKey += "{Numpad3}"; break;
                    case "numpad4": ahkKey += "{Numpad4}"; break;
                    case "numpad5": ahkKey += "{Numpad5}"; break;
                    case "numpad6": ahkKey += "{Numpad6}"; break;
                    case "numpad7": ahkKey += "{Numpad7}"; break;
                    case "numpad8": ahkKey += "{Numpad8}"; break;
                    case "numpad9": ahkKey += "{Numpad9}"; break;
                    case "num0": ahkKey += "{Numpad0}"; break;
                    case "num1": ahkKey += "{Numpad1}"; break;
                    case "num2": ahkKey += "{Numpad2}"; break;
                    case "num3": ahkKey += "{Numpad3}"; break;
                    case "num4": ahkKey += "{Numpad4}"; break;
                    case "num5": ahkKey += "{Numpad5}"; break;
                    case "num6": ahkKey += "{Numpad6}"; break;
                    case "num7": ahkKey += "{Numpad7}"; break;
                    case "num8": ahkKey += "{Numpad8}"; break;
                    case "num9": ahkKey += "{Numpad9}"; break;
                    case "numpadadd":
                    case "numpad+": ahkKey += "{NumpadAdd}"; break;
                    case "numpadsub":
                    case "numpad-": ahkKey += "{NumpadSub}"; break;
                    case "numpadmult":
                    case "numpad*": ahkKey += "{NumpadMult}"; break;
                    case "numpaddiv":
                    case "numpad/": ahkKey += "{NumpadDiv}"; break;
                    case "numpadenter": ahkKey += "{NumpadEnter}"; break;
                    case "numpaddot":
                    case "numpad.": ahkKey += "{NumpadDot}"; break;

                    // Punctuation and symbol keys
                    case "comma": ahkKey += "{,}"; break;
                    case "semicolon": ahkKey += "{;}"; break;
                    case "backslash": ahkKey += "{\\}"; break;
                    case "forwardslash": ahkKey += "{/}"; break;
                    case "hash":
                    case "hashtag": ahkKey += "{#}"; break;
                    case "quote":
                    case "quotemark":
                    case "speechmark": ahkKey += "{\"}"; break; // Only allow 'quote or speechmark' keyword, not direct "
                    //
                    case "period":
                    case "fullstop":
                    case ".": ahkKey += "{.}"; break;
                    case "colon":
                    case ":": ahkKey += "{:}"; break;
                    case "apostrophe":
                    case "'": ahkKey += "{'}"; break;
                    case "backtick":
                    case "`": ahkKey += "{`}"; break;
                    case "openbracket":
                    case "leftbracket":
                    case "[": ahkKey += "{[}"; break;
                    case "closebracket":
                    case "rightbracket":
                    case "]": ahkKey += "{]}"; break;
                    case "openbrace":
                    case "leftbrace":
                    case "{": ahkKey += "{{}"; break;
                    case "closebrace":
                    case "rightbrace":
                    case "}": ahkKey += "{}}"; break;
                    case "pipe":
                    case "|": ahkKey += "{|}"; break;
                    case "dash":
                    case "minus":
                    case "-": ahkKey += "{-}"; break;
                    case "equals":
                    case "=": ahkKey += "{=}"; break;
                    case "plus":
                    case "+": ahkKey += "{+}"; break;
                    case "underscore":
                    case "_": ahkKey += "{_}"; break;
                    case "at":
                    case "@": ahkKey += "{@}"; break;
                    case "pound":
                    case "pounds":
                    case "£": ahkKey += "{£}"; break;
                    case "dollar":
                    case "dollars":
                    case "$": ahkKey += "{$}"; break;
                    case "percent":
                    case "%": ahkKey += "{%}"; break;
                    case "caret":
                    case "^": ahkKey += "{^}"; break;
                    case "ampersand":
                    case "&": ahkKey += "{&}"; break;
                    case "asterisk":
                    case "*": ahkKey += "{*}"; break;
                    case "question":
                    case "?": ahkKey += "{?}"; break;
                    case "exclamation":
                    case "!": ahkKey += "{!}"; break;
                    case "less":
                    case "lessthan":
                    case "<": ahkKey += "{<}"; break;
                    case "greater":
                    case "greaterthan":
                    case ">": ahkKey += "{>}"; break;
                    default:
                        string key = p;
                        if (key.StartsWith("{") && key.EndsWith("}"))
                            key = key.Substring(1, key.Length - 2);
                        if (key.Length >= 2 && key.StartsWith("f", StringComparison.OrdinalIgnoreCase) &&
                            int.TryParse(key.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out int fnum) && fnum >= 1 && fnum <= 24)
                        {
                            ahkKey += "{" + key.ToUpperInvariant() + "}";
                        }
                        else if (key.Length == 1)
                        {
                            ahkKey += key.ToLowerInvariant();
                        }
                        else
                        {
                            ahkKey += "{" + key + "}";
                        }
                        break;
                }
            }
            return ahkKey;
        }

        // Helper: splits keyString on +, but keeps quoted substrings together
        public static List<string> SplitKeyStringRespectingQuotes(string input)
        {
            var result = new List<string>();
            bool inQuotes = false;
            int start = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (input[i] == '+' && !inQuotes)
                {
                    if (start < i)
                        result.Add(input.Substring(start, i - start));
                    start = i + 1;
                }
            }
            if (start < input.Length)
                result.Add(input.Substring(start));
            return result;
        }
    }
}
