using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace КГ_2
{
    public class Polygon
    {
        Point3D fv0, fv1, fv2;
        public Color color;

        public Polygon(Point3D av0, Point3D av1, Point3D av2, Color aColor)
        {
            fv0 = av0;
            fv1 = av1;
            fv2 = av2;
            color = aColor;
        }

        public Point3D v0
        {
            get { return fv0; }
        }

        public Point3D v1
        {
            get { return fv1; }
        }

        public Point3D v2
        {
            get { return fv2; }
        }

        // возвращает вектор нормали для плоскости
        public Point3D normal
        {
            get { return v1.minus(v0).getVectorMult(v2.minus(v0)); }
        }

        public Point3D normalCam
        {
            get { return v1.Vcam.minus(v0.Vcam).getVectorMult(v2.Vcam.minus(v0.Vcam)); }
        }
    }
}

