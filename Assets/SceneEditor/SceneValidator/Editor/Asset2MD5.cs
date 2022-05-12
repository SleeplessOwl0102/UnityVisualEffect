using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class YamlTool
{


    public static class Asset2MD5
    {

        public static string ComputeAssetHash(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return ComputeAssetHash(path);
        }

        public static string ComputeAssetHash(string assetPath)
        {
            if (!File.Exists(assetPath))
                return null;

            List<byte> list = new List<byte>();

            //��ȡ��Դ����meta�ļ�Ϊ�ֽ�����
            list.AddRange(GetAssetBytes(assetPath));
            /*
            //��ȡ��Դ���������meta�ļ�Ϊ�ֽ�����(�������Ҳ����Դ��·��)
            string[] dependencies = AssetDatabase.GetDependencies(assetPath);
            for (int i = 0, iMax = dependencies.Length; i < iMax; ++i)
                list.AddRange(GetAssetBytes(dependencies[i]));
            */
            //�����Դ������������Ļ���Ҳ��Ҫ�����Ӧ���ֽ������ȡ�� list �У�Ȼ���ٽ��� ��ϣ�� �ļ���

            //������Դ hash
            return ComputeHash(list.ToArray());
        }

        private static byte[] GetAssetBytes(string assetPath)
        {
            if (!File.Exists(assetPath))
                return null;

            List<byte> list = new List<byte>();

            var assetBytes = File.ReadAllBytes(assetPath);
            list.AddRange(assetBytes);

            string metaPath = assetPath + ".meta";
            var metaBytes = File.ReadAllBytes(metaPath);
            list.AddRange(metaBytes);

            return list.ToArray();
        }

        private static MD5 md5 = null;
        private static MD5 MD5
        {
            get
            {
                if (null == md5)
                    md5 = MD5.Create();
                return md5;
            }
        }

        private static string ComputeHash(byte[] buffer)
        {
            if (null == buffer || buffer.Length < 1)
                return "";

            byte[] hash = MD5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();

            foreach (var b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

    }
}
