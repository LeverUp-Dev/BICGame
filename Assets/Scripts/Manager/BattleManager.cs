using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Hypocrites.Manager
{
    using DB;
    using DB.Data;
    using Defines;
    using UI.BattleUI;
    using Skill;

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public GraphicRaycaster graphicRaycaster;
        public BattleUI battleUI;

        // 필드 존재 관련
        List<Enemy> enemies;
        List<Member> members;

        // 스킬 타겟팅 관련
        bool isTargeting;
        Camera battleCamera;
        List<Being> targets;
        Member caster;
        Skill castingSkill;

        LayerMask enemyLayer;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            isTargeting = false;

            enemyLayer = LayerMask.GetMask("Enemy");
        }

        void Start()
        {
            enemies = new List<Enemy>
            {
                Database.Instance.Enemies[0],
                Database.Instance.Enemies[1]
            };

            members = new List<Member>
            {
                Database.Instance.Members[0],
                Database.Instance.Members[1]
            };

            targets = new List<Being>();
        }

        void Update()
        {
            if (isTargeting && Input.GetMouseButtonDown(0))
            {
                /*List<RaycastResult> results = new List<RaycastResult>();
                PointerEventData ped = new PointerEventData(null);

                ped.position = Input.mousePosition;

                graphicRaycaster.Raycast(ped, results);*/

                if (battleCamera == null)
                    battleCamera = GameObject.FindWithTag("BattleCamera").GetComponent<Camera>();

                Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * 300);
                if (Physics.Raycast(ray, out RaycastHit hit, 300, enemyLayer))
                {
                    Collider collider = hit.collider;

                    if (!collider.CompareTag("Target"))
                        return;

                    Being target = null;
                    string targetName = collider.name;

                    if (castingSkill.TargetingType == SkillTargetingType.ONE_ENEMY)
                    {
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            if (enemies[i].Name == targetName)
                            {
                                target = enemies[i];
                                break;
                            }
                        }
                    }
                    else if (castingSkill.TargetingType == SkillTargetingType.ONE_PARTY)
                    {
                        for (int i = 0; i < members.Count; i++)
                        {
                            if (members[i].Name == targetName)
                            {
                                target = members[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"{castingSkill.Name}({castingSkill.TargetingType})는 타겟팅 절차가 없는 스킬입니다.");
                    }

                    targets.Add(target);
                    isTargeting = false;
                }
            }
        }

        #region 스킬 사용 관련
        public delegate void SetCooltimeUI(int index);
        public void UseSkill(Member caster, int index, SetCooltimeUI setCooltimeUI)
        {
            // TODO : 타겟팅 도중엔 스킬 버튼을 비활성화, 타겟 대상에 외곽선 효과 부여
            if (isTargeting)
                return;

            if (caster == null)
            {
                Debug.LogError($"MemberSkillSlotUI에 동료 정보가 존재하지 않습니다.");
                return;
            }

            if (members.Find(member => member == caster) == null)
            {
                Debug.LogError($"BattleManager에 MemberSkillSlotUI가 가지고 있는 {caster.Name} 동료가 존재하지 않습니다.");
                return;
            }

            if (index < 0 || index >= 4)
            {
                Debug.LogError($"{index}는 존재하지 않는 스킬 슬롯입니다.");
                return;
            }

            Skill skill = caster.SkillSlot[index];
            if (skill == null)
                return;

            targets.Clear();

            // 타겟팅이 필요한 스킬인 경우 타겟팅 시작
            if (skill.TargetingType == SkillTargetingType.ONE_ENEMY || skill.TargetingType == SkillTargetingType.ONE_PARTY)
            {
                isTargeting = true;
                this.caster = caster;
                castingSkill = skill;

                WaitTargeting(index, setCooltimeUI);
            }
            // 타겟팅이 필요하지 않은 스킬인 경우 바로 스킬 시전
            else
            {
                if (skill.TargetingType == SkillTargetingType.ALL_PARTY || skill.TargetingType == SkillTargetingType.ALL)
                    targets.AddRange(members);

                if (skill.TargetingType == SkillTargetingType.ALL_ENEMY || skill.TargetingType == SkillTargetingType.ALL)
                    targets.AddRange(enemies);

                setCooltimeUI(index);
                caster.UseSkill(skill, targets.ToArray());
            }
        }
        
        async void WaitTargeting(int index, SetCooltimeUI setCooltimeUI)
        {
            await Task.Run(() =>
            {
                while (isTargeting)
                    Thread.Sleep(50);

                setCooltimeUI(index);
                caster.UseSkill(castingSkill, targets.ToArray());

                caster = null;
                castingSkill = null;
            });
        }
        #endregion

        #region 전투 돌입 및 종료 관련
        public void BeginBattle()
        {
            battleUI.Initialize(members.ToArray(), enemies.ToArray());
            battleUI.gameObject.SetActive(true);
            GameSceneManager.Instance.LoadScene(BattleDefines.BATTLE_SCENE_NAME, true);
        }

        public void EndBattle()
        {
            battleUI.gameObject.SetActive(false);
            GameSceneManager.Instance.UnloadScene(BattleDefines.BATTLE_SCENE_NAME);
        }
        #endregion
    }
}
