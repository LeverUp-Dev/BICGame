using UnityEngine;

namespace Hypocrites.Map.Elements
{
    public class Triangle
    {
        public Room R1 { get; }
        public Room R2 { get; }
        public Room R3 { get; }

        public Vector3 circumCenter;
        public float circumRadius; // ������ �������� ����

        public Triangle(Room _p1, Room _p2, Room _p3)
        {
            R1 = _p1;
            R2 = _p2;
            R3 = _p3;

            CalculateCircumCircle();
        }

        /* 
         * �ﰢ�� p1p2p3�� �ܽ� ���
         * : ���� p1p2�� ���� p2p3�� ���� �����̵���ϴ� �� ������ �����Ͽ� �ܽ� ��ǥ ���
         * 1. m1, m2 : ���� p1p2�� p2p3�� ����
         * 2. mp1, mp2 : m1, m2�� ������ �� -1�� �Ǵ� ���� (���� * ���Ⱑ -1�̸� �� ������ ����)
         * 3. �����̵���ϴ� �� ������ ���� y = mp1x + b1, y = mp2x + b2�� ����
         * 4. �� ������ �����Ŀ� ���� ���� x�� y�� ���� p1p2�� ����, ���� p2p3�� ������ ������ b1�� b2�� ���� ����
         * 5. 3�� �� ������ �����Ͽ� x�� ���� ���� �� 4���� ���� b1�� ���� => x ����
         * 6. 5���� ���� x�� 4���� ���� b2�� y = mp1x + b1�� ���� => y ����
         * !. ����Ƽ���� y�� z�� �ش���
         */
        public void CalculateCircumCircle()
        {
            Vector3 p1 = R1.Position;
            Vector3 p2 = R2.Position;
            Vector3 p3 = R3.Position;

            // �� ���� ��ȭ�� ���
            float dx1 = p2.x - p1.x, dx2 = p3.x - p2.x, dx3 = p1.x - p3.x;
            float dy1 = p2.z - p1.z, dy2 = p3.z - p2.z, dy3 = p1.z - p3.z;

            bool isM1Zero = dx1 == 0 || dy1 == 0;
            bool isM2Zero = dx2 == 0 || dy2 == 0;
            bool isM3Zero = dx3 == 0 || dy3 == 0;

            // �����ﰢ���� ��� ������ ������ �ܽ����� ����
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

                // ���� p1p2�� ���Ⱑ ���� ��� ���� p2p3, p3p1�� �̿�
                if (isM1Zero)
                {
                    p1 = R2.Position;
                    p2 = R3.Position;
                    p3 = R1.Position;

                    isChanged = true;
                }
                // ���� p2p3�� ���Ⱑ ���� ��� ���� p3p1, p1p2�� �̿�
                else if (isM2Zero)
                {
                    p1 = R3.Position;
                    p2 = R1.Position;
                    p3 = R2.Position;

                    isChanged = true;
                }

                // x�� y�� ��ȭ�� �ٽ� ���
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

            // 1. �ﰢ�� ABC�� ���� ��, AB, BC, CA�� AP, BP, CP�� ���� ����
            // 2. �� �������� ���� ������ y�� ��� ����̰ų� �����̸� �ﰢ�� ���ο� ����
            // (�� ���� �������� �� P�� �������� ���������� Ȯ��)
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
            // ���� ������ �ȿ� �ִ��� �˻�
            return Mathf.Pow(circumCenter.x - p.x, 2) + Mathf.Pow(circumCenter.z - p.z, 2) <= circumRadius;
        }
    }
}
