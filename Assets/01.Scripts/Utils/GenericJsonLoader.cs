using UnityEngine;

namespace _01.Scripts.Utils
{
    public static class GenericJsonLoader
    {
        /// <summary>
        /// Resources 폴더 내의 지정된 경로에서 JSON 파일을 로드하여 제네릭 타입 T로 파싱합니다.
        /// </summary>
        /// <typeparam name="T">파싱할 데이터의 타입</typeparam>
        /// <param name="fileName">Resources 폴더 내의 파일 이름 (확장자 제외)</param>
        /// <returns>파싱된 객체 (파일이 없거나 파싱 실패 시 기본값 반환)</returns>
        public static T LoadJson<T>(string fileName)
        {
            TextAsset jsonText = Resources.Load<TextAsset>(fileName);
            if (jsonText == null)
            {
                DebugLogger.LogError($"JSON 파일을 찾을 수 없습니다: {fileName}");
                return default;
            }

            try
            {
                T data = JsonUtility.FromJson<T>(jsonText.text);
                return data;
            }
            catch (System.Exception ex)
            {
                DebugLogger.LogError($"JSON 파싱 실패: {ex.Message}");
                return default;
            }
        }
    }
}