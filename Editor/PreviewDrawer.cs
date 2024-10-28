using System.ComponentModel;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphlit
{
    public enum PreviewType
    {
        Inherit = 0,
        Preview2D = 1,
        Preview3D = 2,
    }

    public class PreviewDrawer : ImmediateModeElement
    {
        static Shader _defaultShader = Shader.Find("Unlit/Color");
        internal Shader _previewShader = null;
        ObjectRc<Shader> _shaderRc = null;
        int _resolution = 96;
        Material _material;
        ShaderGraphView _graphView;
        ShaderNode _node;
        bool _disabled = false;
        public int previewId;

        PreviewDrawer _extensionPreviewDrawer;

        public PreviewDrawer GetExtensionPreview(ShaderNode node)
        {
            _extensionPreviewDrawer ??= new PreviewDrawer(node, _graphView, 389)
            {
                _previewShader = _previewShader,
                previewId = previewId
            };

            return _extensionPreviewDrawer;
        }

        public PreviewDrawer(ShaderNode node, ShaderGraphView graphView, int resolution = 96)
        {
            _graphView = graphView;
            _node = node;
            _material = graphView.PreviewMaterial;
            _resolution = resolution;
            style.width = _resolution;
            style.height = _resolution;
            //cullingEnabled = true;

            name = "PreviewDrawer";
        }

        public bool HasShader => _previewShader != null;
        public void SetShader(ObjectRc<Shader> shader)
        {
            _shaderRc?.Drop();
            _shaderRc = shader;

            _previewShader = _shaderRc.Clone();
            if (_extensionPreviewDrawer is not null)
            {
                _extensionPreviewDrawer._previewShader = _previewShader;
                _extensionPreviewDrawer.previewId = previewId;
            }
            MarkDirtyRepaint();
        }

        public void Disable()
        {
            _disabled = true;
            style.height = 0;
            style.width = 0;
        }

        public void Enable()
        {
            _disabled = false;
            style.width = _resolution;
            style.height = _resolution;
        }

        int _graphTimeId = Shader.PropertyToID("_GraphTime");
        int _preview3dId = Shader.PropertyToID("_Preview3D");
        int _previewIDId = Shader.PropertyToID("_PreviewID");

        protected override void ImmediateRepaint()
        {
            if (_disabled)
            {
                return;
            }

            if (!_material)
            {
                _material = _graphView.PreviewMaterial;
                return;
            }

            _material.shader = HasShader ? _previewShader : _defaultShader;

            if (_material.shader == null)
            {
                return;
            }

            float t = Time.realtimeSinceStartup;

            Vector4 time = new(t / 20.0f, t, t * 2.0f, t * 3.0f);
            //Vector4 timeParameters = new Vector4(t, Mathf.Sin(t), Mathf.Cos(t), 0.0f);
            var previewType = _node.DefaultPreview;
            if (previewType == PreviewType.Inherit)
            {
                previewType = _node._inheritedPreview;
            }
            _material.SetInteger(_previewIDId, previewId);
            _material.SetVector(_graphTimeId, time);
            _material.SetFloat(_preview3dId, previewType == PreviewType.Preview3D ? 1f : 0f);
            Graphics.DrawTexture(contentRect, Texture2D.whiteTexture, _material, 0);

            Repaint();
        }

        async void Repaint()
        {
            await Task.Delay(16);
            MarkDirtyRepaint();
        }
    }
}