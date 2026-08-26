using RoundsWithBots.Menu;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoundsWithBots.Utils.Debug {
    public static class Profiler {
        private static readonly Stack<ProfilerSection> sectionStack =
            new Stack<ProfilerSection>();
        private static readonly List<ProfilerSection> rootSections =
            new List<ProfilerSection>();

        private class ProfilerSection {
            public string Name;
            public Stopwatch Stopwatch;
            public List<ProfilerSection> Children;

            public ProfilerSection(string name) {
                Name = name;
                Stopwatch = Stopwatch.StartNew();
                Children = new List<ProfilerSection>();
            }
        }

        public static void Start(string name) {
            if (!RWBMenu.EnableProfiler.Value) return;

            ProfilerSection section = new ProfilerSection(name);

            if (sectionStack.Count > 0) {
                sectionStack.Peek().Children.Add(section);
            } else {
                rootSections.Add(section);
            }

            sectionStack.Push(section);
        }

        public static void End(string name) {
            if (!RWBMenu.EnableProfiler.Value) return;

            if (sectionStack.Count == 0) {
                LoggerUtils.Log(BepInEx.Logging.LogLevel.Warning,
                    $"Profiler.End(\"{name}\") called without a matching Start()."
                );

                return;
            }

            ProfilerSection section = sectionStack.Pop();

            if (section.Name != name) {
                LoggerUtils.Log(BepInEx.Logging.LogLevel.Warning,
                    $"Profiler section mismatch. " +
                    $"Expected End(\"{section.Name}\"), got End(\"{name}\")."
                );
            }

            section.Stopwatch.Stop();
        }

        public static void Log() {
            if (!RWBMenu.EnableProfiler.Value) return;

            foreach (ProfilerSection section in rootSections) {
                LogSection(section, 0);
            }

            rootSections.Clear();
        }

        private static void LogSection(ProfilerSection section, int depth) {
            string indentation = new string(' ', depth * 2);

            LoggerUtils.Log(BepInEx.Logging.LogLevel.Debug,
                $"{indentation}{section.Name}: " +
                $"{section.Stopwatch.Elapsed.TotalMilliseconds:F2} ms"
            );

            foreach (ProfilerSection child in section.Children) {
                LogSection(child, depth + 1);
            }
        }
    }
}