using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using System.Collections.Generic;

public class DashedLineDrawer : ImmediateModeShapeDrawer
{
    public List<Vector3> pointList = new List<Vector3>();
    public List<Color> colorList = new List<Color>();
    public float m_thickness = 0.125f;
    public DashStyle m_dashStyle = DashStyle.RelativeDashes(DashType.Rounded, 0.2f, 0.2f, DashSnapping.Tiling, 0, 0);


    public void SetPoints(List<Vector3> points, List<Color> colors)
    {
        pointList = points;
        colorList = colors;
    }

    public override void DrawShapes(Camera cam)
    {
        if (pointList.Count == 0)
        {
            return;
        }

        using (Draw.Command(cam))
        {
            Draw.LineGeometry = LineGeometry.Billboard;
            Draw.ThicknessSpace = ThicknessSpace.Meters;

            Draw.UseDashes = true;
            Draw.DashStyle = m_dashStyle;

            for (int i = 0; i < pointList.Count - 1; i++)
            {
                Draw.Line(pointList[i], pointList[i + 1], m_thickness, colorList[i], colorList[i + 1]);
            }
        }
    }
}