using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RoundsWithBots.Extensions {
    [Serializable]
    public class CharacterDataAdditionalData {
        public bool IsBot { get; set; }

        public CharacterDataAdditionalData() {
            IsBot = false;
        }
    }
    public static class CharacterDataExtension {
        public static readonly ConditionalWeakTable<CharacterData, CharacterDataAdditionalData> data =
            new ConditionalWeakTable<CharacterData, CharacterDataAdditionalData>();

        public static CharacterDataAdditionalData GetAdditionalData(this CharacterData characterData) {
            return data.GetOrCreateValue(characterData);
        }

        public static void AddData(this CharacterData characterData, CharacterDataAdditionalData value) {
            try {
                data.Add(characterData, value);
            } catch(Exception) { }
        }
    }
}
