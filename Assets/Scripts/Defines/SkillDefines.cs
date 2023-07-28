using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Defines
{
    public enum SkillType
    {
        PHYSICAL,   // ����
        MAGICAL     // ����
    }

    public enum SkillAttackType
    {
        SINGLE,         // ��߼�
        MULTIPLE,       // ��ȸ��
        DOT,            // ��Ʈ ������
        COUNT_LIMIT,    // Ƚ�� ����
        UTILITY         // Ư�� �ൿ
    }

    public enum SkillTargetingType
    {
        ALL,        // ��ü
        ALL_ENEMY,  // ��ü (��)
        ALL_PARTY,  // ��ü(�Ʊ�)
        ONE_ENEMY,  // Ÿ����(��)
        ONE_PARTY,  // Ÿ����(�Ʊ�)
        ITSELF,     // ����
        NONE        // ������
    }
}
