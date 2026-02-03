using System;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Inherit this class to quickly create a custom Editor for an OpenXR feature with a wider label width.
    /// </summary>
    abstract class OpenXRFeatureEditorBase : Editor
    {
        // Override this label width in your inherited class if needed
        protected float m_LabelWidth = 230f;

        public override void OnInspectorGUI()
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = m_LabelWidth;
            base.OnInspectorGUI();
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
