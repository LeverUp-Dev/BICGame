using UnityEngine;

namespace Hypocrites.Map.Elements
{
    public class Triangle
    {
        public Room R1 { get; }
        public Room R2 { get; }
        public Room R3 { get; }

        public Vector3 circumCenter;
        public float circumRadius; // 외접원 반지름의 제곱

        public Triangle(Room _p1, Room _p2, Room _p3)
        {
            R1 = _p1;
            R2 = _p2;
            R3 = _p3;

            CalculateCircumCircle();
        }

        /* 
         * 삼각형 p1p2p3의 외심 계산
         * : 선분 p1p2와 선분 p2p3을 각각 수직이등분하는 두 직선을 연립하여 외심 좌표 계산
         * 1. m1, m2 : 선분 p1p2와 p2p3의 기울기
         * 2. mp1, mp2 : m1, m2와 곱했을 때 -1이 되는 기울기 (기울기 * 기울기가 -1이면 두 선분은 직교)
         * 3. 수직이등분하는 두 직선을 각각 y = mp1x + b1, y = mp2x + b2로 지정
         * 4. 두 직선의 방정식에 대해 각각 x와 y에 선분 p1p2의 중점, 선분 p2p3의 중점을 대입해 b1과 b2에 대해 정리
         * 5. 3의 두 직선을 연립하여 x에 대해 정리 후 4에서 나온 b1을 대입 => x 도출
         * 6. 5에서 나온 x와 4에서 나온 b2를 y = mp1x + b1에 대입 => y 도출
         * !. 유니티에서 y는 z에 해당함
         */
        public void CalculateCircumCircle()
        {
            Vector3 p1 = R1.Position;
            Vector3 p2 = R2.Position;
            Vector3 p3 = R3.Position;

            // 각 변의 변화량 계산
            float dx1 = p2.x - p1.x, dx2 = p3.x - p2.x, dx3 = p1.x - p3.x;
            float dy1 = p2.z - p1.z, dy2 = p3.z - p2.z, dy3 = p1.z - p3.z;

            bool isM1Zero = dx1 == 0 || dy1 == 0;
            bool isM2Zero = dx2 == 0 || dy2 == 0;
            bool isM3Zero = dx3 == 0 || dy3 == 0;

            // 직각삼각형일 경우 빗변의 중점을 외심으로 지정
            if ((isM3Zero && (isM1Zero || isM2Zero)) || (isM1Zero && isM2Zero))
            {
                if (!isM2Zero)
                {
                    p1 = R2.Position;
                    p2 = R3.Position;
                }
                else if (!isM3Zero)
                {
                    p1 = R3.Position;
                    p2 = R1.Position;
                }

                circumCenter = new Vector3((p1.x + p2.x) / 2, 0, (p1.z + p2.z) / 2);
            }
            else
            {
                bool isChanged = false;

                // 선분 p1p2의 기울기가 없을 경우 선분 p2p3, p3p1을 이용
                if (isM1Zero)
                {
                    p1 = R2.Position;
                    p2 = R3.Position;
                    p3 = R1.Position;

                    isChanged = true;
                }
                // 선분 p2p3의 기울기가 없을 경우 선분 p3p1, p1p2를 이용
                else if (isM2Zero)
                {
                    p1 = R3.Position;
                    p2 = R1.Position;
                    p3 = R2.Position;

                    isChanged = true;
                }

                // x와 y의 변화량 다시 계산
                if (isChanged)
                {
                    dx1 = p2.x - p1.x;
                    dx2 = p3.x - p2.x;
                    dy1 = p2.z - p1.z;
                    dy2 = p3.z - p2.z;
                }

                float m1 = dy1 / dx1;
                float m2 = dy2 / dx2;
                float mp1 = (m1 != 0) ? -(1 / m1) : 0;
                float mp2 = (m2 != 0) ? -(1 / m2) : 0;

                float x;
                float y;

                x = p2.x / 2 + (mp1 * p1.x - mp2 * p3.x - p1.z + p3.z) / (2 * (mp1 - mp2));
                y = mp2 * x + (-mp2 * (p2.x + p3.x) + p2.z + p3.z) / 2;

                circumCenter = new Vector3(x, 0, y);
            }

            circumRadius = Mathf.Pow(circumCenter.x - p1.x, 2) + Mathf.Pow(circumCenter.z - p1.z, 2);
        }

        public bool Contains(Vector3 p)
        {
            Vector3 p1 = R1.Position;
            Vector3 p2 = R2.Position;
            Vector3 p3 = R3.Position;

            // 1. 삼각형 ABC가 있을 때, AB, BC, CA와 AP, BP, CP를 각각 외적
            // 2. 각 외적으로 나온 벡터의 y가 모두 양수이거나 음수이면 삼각형 내부에 있음
            // (각 변을 기준으로 점 P가 왼쪽인지 오른쪽인지 확인)
            Vector3 p1p = new Vector3(p.x - p1.x, p.y - p1.y, p.z - p1.z);
            Vector3 p1p2 = new Vector3(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
            bool side1 = Vector3.Cross(p1p, p1p2).y >= 0;

            Vector3 p2p = new Vector3(p.x - p2.x, p.y - p2.y, p.z - p2.z);
            Vector3 p2p3 = new Vector3(p3.x - p2.x, p3.y - p2.y, p3.z - p2.z);
            bool side2 = Vector3.Cross(p2p, p2p3).y >= 0;

            Vector3 p3p = new Vector3(p.x - p3.x, p.y - p3.y, p.z - p3.z);
            Vector3 p3p1 = new Vector3(p1.x - p3.x, p1.y - p3.y, p1.z - p3.z);
            bool side3 = Vector3.Cross(p3p, p3p1).y >= 0;

            if (side1 == side2 == side3)
                return true;
            else
                return false;
        }
        
        public bool CircumCircleContains(Vector3 p)
        {
            // 점이 외접원 안에 있는지 검사
            return Mathf.Pow(circumCenter.x - p.x, 2) + Mathf.Pow(circumCenter.z - p.z, 2) <= circumRadius;
        }
    }
}
