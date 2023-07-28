using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.Defines
{
    public enum SkillType
    {
        PHYSICAL,   // 물리
        MAGICAL     // 마법
    }

    public enum SkillAttackType
    {
        SINGLE,         // 즉발성
        MULTIPLE,       // 다회성
        DOT,            // 도트 데미지
        COUNT_LIMIT,    // 횟수 제한
        UTILITY         // 특수 행동
    }

    public enum SkillTargetingType
    {
        ALL,        // 전체
        ALL_ENEMY,  // 전체 (적)
        ALL_PARTY,  // 전체(아군)
        ONE_ENEMY,  // 타겟팅(적)
        ONE_PARTY,  // 타겟팅(아군)
        ITSELF,     // 본인
        NONE        // 비전투
    }
}
