
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ZSG
{
    public static class Helpers
    {
        // https://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string
        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static int CountBits(int v)
        {
            int c = 0;
            while (v != 0)
            {
                v &= (v - 1);
                c++;
            }

            return c;
        }

        public static string AssetSerializableReference(UnityEngine.Object @object)
        {
            ObjectIdentifier.TryGetObjectIdentifier(@object, out var id);
            return id.localIdentifierInFile + "|" + id.guid.ToString();
        }

        public static T SerializableReferenceToObject<T>(string reference) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(reference))
            {
                return null;
            }
            else if (!reference.Contains('|'))
            {
                return null;
            }
            string[] split = reference.Split('|');
            long localId = long.Parse(split[0]);
            string guid = split[1];

            var path = AssetDatabase.GUIDToAssetPath(guid);


            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

            if (mainAsset is T x)
            {
                return x;
            }

            if (path.StartsWith("Resources/"))
            {
                //Debug.Log(path.Substring("Resources/".Length));
                //asset = AssetDatabase.GetBuiltinExtraResource<T>(path.Substring("Resources/".Length));
                return null;
            }

            T asset = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                .Where(x => x is T).Cast<T>()
                .Where(x =>
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(x, out string guid2, out long localId2);
                    return localId2 == localId;
                }).First();

            return asset;
        }

    }

    public static class PortExtenstions
    {
        public static int GetPortID(this Port port)
        {
            return (int)port.userData;
        }
        public static void SetPortID(this Port port, int id)
        {
            port.userData = id;
        }

        public static PortDescriptor GetByID(this List<PortDescriptor> portDescriptors, int ID)
        {
            return portDescriptors.Where(x => x.ID == ID).First();
        }
    }

}