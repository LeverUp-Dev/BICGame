using System;
using System.IO;
using UnityEngine;

namespace Hypocrites.Utility
{
    public static class JsonIOUtility
    {
        /// <summary>
        /// ��ü�� Json ���Ϸ� ����
        /// </summary>
        /// <param name="path">���ϸ�� Ȯ����(.json)���� ������ ���� ���, "Assets/Data/" ���� ����</param>
        /// <param name="data">Json���� ������ ��ü, Serializable ��ü�� public ��� ������(�Ӽ� �Ұ���) ���� ����</param>
        public static void SaveJson(string path, object data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Json ������ �о�� �����ߴ� ��ü ��ȸ
        /// </summary>
        /// <typeparam name="T">��ü Ÿ��</typeparam>
        /// <param name="path">���ϸ�� Ȯ����(.json)���� ������ ���� ���</param>
        /// <returns>��ȸ�� ��ü ����</returns>
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
