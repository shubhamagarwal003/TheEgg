using FullInspector.Internal;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules.Attributes {
    [CustomAttributePropertyEditor(typeof(CommentAttribute), ReplaceOthers = false)]
    public class CommentAttributeEditor<T> : AttributePropertyEditor<T, CommentAttribute> {
        private static MessageType MapCommentType(CommentType commentType) {
            return (MessageType)commentType;
        }

        protected override T Edit(Rect region, GUIContent label, T element, CommentAttribute attribute) {
            EditorGUI.HelpBox(region, attribute.Comment, MapCommentType(attribute.Type));
            return element;
        }

        protected override float GetElementHeight(GUIContent label, T element, CommentAttribute attribute) {
            float height = CommentUtils.GetCommentHeight(attribute.Comment);

            float minImageHeight = 40;
            if (attribute.Type != CommentType.None && height < minImageHeight) {
                height = minImageHeight;
            }

            return height;
        }
    }
}