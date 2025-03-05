using System;
using System.Collections;
using UnityEngine;

namespace RoundsWithBots.Utils {
    public static class EaseUtils {
        public enum EaseType {
            easeInSine,
            easeOutSine,
            easeInOutSine,
            easeInQuad,
            easeOutQuad,
            easeInOutQuad,
        }

        public static float Ease(float time, float start, float end, float duration, EaseType easeType) {
            switch(easeType) {
                case EaseType.easeInSine:
                    return -Mathf.Cos((time / duration) * (Mathf.PI / 2)) + 1 * (end - start) + start;
                case EaseType.easeOutSine:
                    return Mathf.Sin((time / duration) * (Mathf.PI / 2)) * (end - start) + start;
                case EaseType.easeInOutSine:
                    return -0.5f * (Mathf.Cos(Mathf.PI * time / duration) - 1) * (end - start) + start;
                case EaseType.easeInQuad:
                    return (end - start) * (time /= duration) * time + start;
                case EaseType.easeOutQuad:
                    return -(end - start) * (time /= duration) * (time - 2) + start;
                case EaseType.easeInOutQuad:
                    return ((time /= duration / 2) < 1) ? (end - start) / 2 * time * time + start : -(end - start) / 2 * ((--time) * (time - 2) - 1) + start;
                default:
                    return 0;
            }
        }

        public static IEnumerator EaseCoroutine(float duration, EaseType easeType, Action<float> action, Action onComplete = null, float min = 0, float max = 1) {
            float time = 0;
            while(time < duration) {
                action(Ease(time, min, max, duration, easeType));
                time += Time.deltaTime;
                yield return null;
            }
            action(Ease(duration, min, max, duration, easeType));
            onComplete?.Invoke();
        }
    }
}
