using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace КГ_2
{
    public class Camera
    {
        Point3D fpos; //расположение камеры
        Point3D fdirection;  //направление камеры
        double fangle; //угол обзора камеры в радианах
        public Matrix toRotate;

        public Camera(Point3D _pos, Point3D _direction, double _angle)
        {
            fpos = _pos;
            fdirection = _direction;
            if (direction.length == 0)
            {
                direction.applyMatrix(Matrix.getShiftMatr(0, 0, 1)); //если вектор направления нулевой, то мы его в сторону оси z направляем
            }
            fangle = _angle * Math.PI / 180; //переводим угол обзора из градусов в радианы
            calcMatrixToRotate();
        }

        public double angle
        {
            get { return fangle; }
        }

        public Point3D pos
        {
            get { return fpos; }
        }

        public Point3D direction
        {
            get { return fdirection; }
        }

        // Процедура высчитывает матрицу преобразования координатных осей так, чтобы ось Z смотрела в направлении камеры
        void calcMatrixToRotate()
        {
            Matrix shift = Matrix.getShiftMatr(-pos.getX(), -pos.getY(), -pos.getZ()); // для переноса начала координат в точку с камерой
            Point3D proj = new Point3D(direction.getX(), direction.getY(), 0); // проекция направления камеры на плоскость ХОY
            double ang = proj.findAngle(new Point3D(0, 1, 0)); //угол между OY и этой проекцией
            if (proj.getX() < 0)
                ang = -ang;
            Matrix rotZ = Matrix.getRorateZMatr(ang); // поворот вокруг OZ, чтобы OY совпала с этой проекцией
            Matrix rotX = Matrix.getRorateXMatr(direction.findAngle(new Point3D(0, 0, 1))); //а это матрица поворота, чтобы Z совпало с направлением камеры

            // вычисляется конечная матрица, с помощью которой можно преобразоватькоординаты объекта в соответствии с положением и направлением камеры
            toRotate = shift.mulMatrs(rotZ.mulMatrs(rotX));
        }
    }
}

