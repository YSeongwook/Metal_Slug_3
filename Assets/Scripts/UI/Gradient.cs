using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    public Color32 topColor = Color.white;
    public Color32 bottomColor = Color.black;

    public override void ModifyMesh(VertexHelper helper)
    {
        if (!IsActive() || helper.currentVertCount == 0)
            return;

        List<UIVertex> vertices = new List<UIVertex>();
        helper.GetUIVertexStream(vertices);

        float centerY = (vertices[0].position.y + vertices[vertices.Count - 1].position.y) / 2f; // UI 요소의 중심점

        UIVertex v = new UIVertex();

        for (int i = 0; i < helper.currentVertCount; i++)
        {
            helper.PopulateUIVertex(ref v, i);
            // UI 요소의 위쪽과 아래쪽에 따라 색상 설정
            if (v.position.y >= centerY)
            {
                v.color = topColor;
            }
            else
            {
                v.color = bottomColor;
            }
            helper.SetUIVertex(v, i);
        }
    }
}
