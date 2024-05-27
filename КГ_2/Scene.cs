using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace КГ_2
{
    public class Scene
    {
        public Color brush = Color.RoyalBlue; // цвет фигуры
        public Bitmap pic; // плоскость для рисования
        public int height, width; // высота и длинна
        public Camera cam; // камера
        double del;
        public List<Point3D> verts; // вершины фигуры
        public List<Polygon> polys; // полигоны
        public Point3D lightPoint; // источник света
        double[,] Zbuffer;

        public Scene(int aheight, int awidth)
        {
            height = aheight;
            width = awidth;
            pic = new Bitmap(width, height);
            verts = new List<Point3D>();
            polys = new List<Polygon>();
        }

        // построение трапеции
        public void addTrapezoid(double height, double radius, int sides)
        {
            double h = 360 / sides;
            List<Point3D> baseDown = new List<Point3D>(); // нижнее основание
            List<Point3D> baseUp = new List<Point3D>(); // верхнее основание
            for (double i = 0; i < 360; i += h)
            {
                double rad = i * Math.PI / 180;
                baseDown.Add(new Point3D(radius * Math.Cos(rad), radius * Math.Sin(rad), 0));
                baseUp.Add(new Point3D(radius * Math.Cos(rad), radius * Math.Sin(rad), height));
            }

            for (int i = 0; i < baseUp.Count - 1; i++) // Добавление поверхности от нижнего основания до верхнего
            {
                polys.Add(new Polygon(baseDown[i], baseDown[i + 1], baseUp[i], brush));
                polys.Add(new Polygon(baseDown[i + 1], baseUp[i + 1], baseUp[i], brush));
            }
            polys.Add(new Polygon(baseDown[baseDown.Count - 1], baseDown[0], baseUp[baseUp.Count - 1], brush)); 
            polys.Add(new Polygon(baseDown[0], baseUp[0], baseUp[baseUp.Count - 1], brush));

            verts.Add(new Point3D(0, 0, height));
            for (int i = 0; i < baseUp.Count - 1; i++) polys.Add(new Polygon(baseUp[i], baseUp[i + 1], verts[0], brush)); // добавление верхнего основания
            polys.Add(new Polygon(baseUp[baseUp.Count - 1], baseUp[0], verts[0], brush));

            verts.Add(new Point3D(0, 0, 0));
            for (int i = 0; i < baseDown.Count - 1; i++) polys.Add(new Polygon(baseDown[i+1], baseDown[i], verts[1], brush)); // добавление нижнего основания
            polys.Add(new Polygon(baseDown[0], baseDown[baseDown.Count - 1], verts[1], brush));

            verts.AddRange(baseUp);
            verts.AddRange(baseDown);
        }

        public void addCamera(Camera acam) // добавляем камеру
        {
            cam = acam;
            del = 1 / Math.Tan(cam.angle / 2);
        }

        // преобразование кординат
        public Point convertToScreenPoint(Point3D v)
        {
            Point result = new Point();
            result.X = (int)Math.Round(v.Vcam.getX() / v.Vcam.getZ() * del * (double)width / 2 + (double)width / 2); //преобразовываем в экранные координаты
            result.Y = (int)Math.Round(v.Vcam.getY() / v.Vcam.getZ() * del * (double)width / 2 + (double)height / 2);
            return result;
        }

        // проверяет, находится ли точка в треугольнике или нет
        public bool CheckPoint(Point pnt, List<Point> pnts)
        {
            Point3D v0, v1, v2;
            // вычисляем вектора для каждой из сторон треугольника
            v0 = new Point3D(pnt).minus(new Point3D(pnts[0]));
            v1 = new Point3D(pnts[1]).minus(new Point3D(pnts[0]));
            v2 = new Point3D(pnts[2]).minus(new Point3D(pnts[0]));

            if (Math.Abs(v2.findAngle(v1) - v2.findAngle(v0) - v1.findAngle(v0)) > 1e-3)
                return false;

            v0 = new Point3D(pnt).minus(new Point3D(pnts[1]));
            v1 = new Point3D(pnts[0]).minus(new Point3D(pnts[1]));
            v2 = new Point3D(pnts[2]).minus(new Point3D(pnts[1]));
            if (Math.Abs(v2.findAngle(v1) - v2.findAngle(v0) - v1.findAngle(v0)) > 1e-3)
                return false;

            return true;
        }

        // Находит точку на грани, которая смотрит в заданную точку на экране
        public Point3D findVertex(Point pnt, Polygon poly)
        {
            // преобразуем точку в точку на экране
            Point3D onScreen = new Point3D((pnt.X - (double)width / 2) / (del * (double)width / 2), (pnt.Y - (double)height / 2) / (del * (double)width / 2), 1);
            Point3D norm = poly.normalCam; // нормаль камеры
            // (A, B, C) - нормаль плоскости, D - коэффициент смещения
            double A = norm.X,
                B = norm.Y,
                C = norm.Z,
                D = -(A * poly.v0.Vcam.getX() + B * poly.v0.Vcam.getY() + C * poly.v0.Vcam.getZ());
            double t = -D / (A * onScreen.getX() + B * onScreen.getY() + C * onScreen.getZ()); // расстояние отначала координат до точки пересечения луча с плоскостью
            return onScreen.scale(t); // возвращаем точку пересечения луча с плоскостью
        }

        public Color colorWithLight(Point3D t, Polygon poly) // вычисляем цвет точки в зависимости от источника освещения
        {
            double R = 20, G = 20, B = 20; //стандартное значение цветов

            Point3D norm = poly.normalCam; // нормаль к плоскости
            Point3D ray = lightPoint.Vcam.minus(t); //от точки до источника освещения
            if (ray.findAngle(norm) < Math.PI / 2) // проверяем находится ли точка в области, освещаемой источникам света
            {
                // пересчитываем значения, чтобы учесть степень освещенности точки в зависимости от угла падения света на нее
                double cos = Math.Pow(Math.Cos(ray.findAngle(norm)), 0.5);
                R = cos * poly.color.R;
                G = cos * poly.color.G;
                B = cos * poly.color.B;
            }
            return Color.FromArgb(Math.Min((int)R, 255), Math.Min((int)G, 255), Math.Min((int)B, 255));
        }

        // рисует полигон на экране
        public void DrawPolygon(Polygon poly)
        {
            // формируем полигон
            List<Point> pnts = new List<Point>();
            pnts.Add(convertToScreenPoint(poly.v0));
            pnts.Add(convertToScreenPoint(poly.v1));
            pnts.Add(convertToScreenPoint(poly.v2));

            //  ищем максимальные и минимальные значения X и Y для границ поликона
            int minX, minY, maxX, maxY;
            minX = maxX = pnts[0].X;
            minY = maxY = pnts[0].Y;
            foreach (Point pnt in pnts)
            {
                minX = Math.Min(minX, pnt.X);
                maxX = Math.Max(maxX, pnt.X);
                minY = Math.Min(minY, pnt.Y);
                maxY = Math.Max(maxY, pnt.Y);
            }
            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);
            maxX = Math.Min(maxX, width - 1);
            maxY = Math.Min(maxY, height - 1);
            for (int X = minX; X <= maxX; X++) // проход по X
                for (int Y = minY; Y <= maxY; Y++) // проход по Y
                {
                    Point curp = new Point(X, Y); // текущая точка
                    if (CheckPoint(curp, pnts)) // если точка принадлежит полигону
                    {
                        Point3D curV = findVertex(curp, poly); // находим точку на грани 
                        if (curV.getZ() < Zbuffer[X, Y])
                        {
                            Zbuffer[X, Y] = curV.getZ();
                            pic.SetPixel(X, Y, colorWithLight(curV, poly)); // рисуем точку
                        }
                    }
                }
        }

        public void Render()
        {
            if (cam != null) // проверяем наличие камеры
            {
                //вычисление проекционных координат
                foreach (Point3D vr in verts)
                    vr.rotateForCam(cam);// вычисляем положение точек относительно камеры
                lightPoint.rotateForCam(cam); // вычисляем положение источника света относительно камеры
                Graphics gr = Graphics.FromImage(pic);
                gr.Clear(Color.White);
                Zbuffer = new double[width, height];
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        Zbuffer[i, j] = 10000;  //устанавливаем линию горизонта

                foreach (Polygon pl in polys)
                    if (pl.v0.minus(cam.pos).findAngle(pl.normal) > Math.PI / 2)  //если плоскость смотрит в сторону камеры
                        if (pl.v0.Vcam.getZ() > 0.2 && pl.v1.Vcam.getZ() > 0.2 && pl.v2.Vcam.getZ() > 0.2) //и если она не лежит за ней
                            DrawPolygon(pl);
            }
        }
    }
}

