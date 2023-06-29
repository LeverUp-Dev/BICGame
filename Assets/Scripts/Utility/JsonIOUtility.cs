using System;
using System.IO;
using UnityEngine;

namespace Hypocrites.Utility
{
    public static class JsonIOUtility
    {
        /// <summary>
        /// 객체를 Json 파일로 저장
        /// </summary>
        /// <param name="path">파일명과 확장자(.json)까지 포함한 파일 경로, "Assets/Data/" 폴더 권장</param>
        /// <param name="data">Json으로 저장할 객체, Serializable 객체의 public 멤버 변수만(속성 불가능) 저장 가능</param>
        public static void SaveJson(string path, object data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Json 파일을 읽어와 저장했던 객체 조회
        /// </summary>
        /// <typeparam name="T">객체 타입</typeparam>
        /// <param name="path">파일명과 확장자(.json)까지 포함한 파일 경로</param>
        /// <returns>조회한 객체 정보</returns>
        public static T LoadJson<T>(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return (T)JsonUtility.FromJson<T>(json);
            }
            
            return default;
        }
    }
}
