using UnityEditor;
using UnityEngine;
using UnityEditor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZSG
{
    public partial class DefaultInspector : ShaderGUI
    {
        void OnGUIStart(MaterialEditor editor, MaterialProperty[] properties)
        {
            _material = (Material)editor.target;
            _shader = _material.shader;

            Initialize(editor, properties);
        }

        bool _start = true;
        Material _material;
        Shader _shader;
        PropertyElement _propertyTree;
        static bool _reinitialize = false;
        public static void Reinitialize() => _reinitialize = true;
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            if (_start || _reinitialize)
            {
                OnGUIStart(editor, properties);
                _start = false;
                _reinitialize = false;
            }


            UserProperties(editor, properties);
            Footer(editor);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            material.shaderKeywords = null;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
        }

        void Initialize(MaterialEditor editor, MaterialProperty[] properties)
        {
            _propertyTree = new PropertyElement(0);
            PropertyElement current = _propertyTree;
            int baseIndentation = EditorGUI.indentLevel;
            for (int i = 0; i < properties.Length; i++)
            {
                MaterialProperty materialProperty = properties[i];
                var flags = materialProperty.flags;

                if (flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                {
                    continue;
                }

                var p = new PropertyElement(i);
                var attributes = _shader.GetPropertyAttributes(i).ToHashSet();

                string tooltip = null;
                foreach (string a in attributes)
                {
                    if (a.StartsWith("Tooltip("))
                    {
                        tooltip = a.Substring("Tooltip(".Length).TrimEnd(')');
                    }
                }


                var guiContent = new GUIContent(materialProperty.displayName, tooltip);

                if (attributes.Contains("Foldout"))
                {
                    p.onGui = (e, m, i) =>
                    {
                        CoreEditorUtils.DrawSplitter();
                        p.hideChildren = CoreEditorUtils.DrawHeaderFoldout(materialProperty.displayName, m[i].floatValue == 0);
                        m[i].floatValue = p.hideChildren ? 0 : 1;
                    };

                    _propertyTree.Add(p);
                    current = p;
                    continue;
                }

                if (materialProperty.type == MaterialProperty.PropType.Texture)
                {
                    p.onGui = (e, m, i) => TextureProperty(e, m[i], m[i].displayName);
                    if (attributes.Contains("Linear"))
                    {
                        p.onGui += (e, m, i) => LinearWarning(m[i]);
                    }
                }
                else if (materialProperty.type == MaterialProperty.PropType.Vector)
                {
                    if (attributes.Contains("Vector2"))
                    {
                        p.onGui = (e, m, i) => Vector2Property(e, m[i], guiContent);
                    }
                    else if (attributes.Contains("Vector3"))
                    {
                        p.onGui = (e, m, i) => Vector3Property(e, m[i], guiContent);
                    }
                    else
                    {
                        p.onGui = (e, m, i) => Vector4Property(e, m[i], guiContent);
                    }
                }
                else
                {
                    p.onGui = (e, m, i) => e.ShaderProperty(m[i], guiContent);
                }

                current.Add(p);
            }
        }

        void UserProperties(MaterialEditor editor, MaterialProperty[] properties)
        {
            foreach (PropertyElement p in _propertyTree.Children)
            {
                p.OnGUI(editor, properties);
            }
        }

        public void Footer(MaterialEditor editor)
        {
            CoreEditorUtils.DrawSplitter();
            EditorGUILayout.Space();
            editor.LightmapEmissionProperty();
            editor.RenderQueueField();
            editor.EnableInstancingField();
            editor.DoubleSidedGIField();
        }

        public class PropertyElement
        {
            public PropertyElement(int index)
            {
                _index = index;
            }
            int _index;
            public bool hideChildren = false;
            public Action<MaterialEditor, MaterialProperty[], int> onGui;
            public List<PropertyElement> Children { get; private set; }
            public PropertyElement Parent { get; private set; }
            public void Add(PropertyElement p)
            {
                Children ??= new List<PropertyElement>();
                p.Parent = this;
                Children.Add(p);
            }

            public void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
            {
                onGui.Invoke(editor, properties, _index);

                if (hideChildren && Children is not null)
                {
                    foreach (PropertyElement p in Children)
                    {
                        p.OnGUI(editor, properties);
                    }
                }
            }
        }
    }
}