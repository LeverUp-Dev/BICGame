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

        // �ʵ� ���� ����
        List<Enemy> enemies;
        List<Member> members;

        // ��ų Ÿ���� ����
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
                        Debug.LogError($"{castingSkill.Name}({castingSkill.TargetingType})�� Ÿ���� ������ ���� ��ų�Դϴ�.");
                    }

                    targets.Add(target);
                    isTargeting = false;
                }
            }
        }

        #region ��ų ��� ����
        public delegate void SetCooltimeUI(int index);
        public void UseSkill(Member caster, int index, SetCooltimeUI setCooltimeUI)
        {
            // TODO : Ÿ���� ���߿� ��ų ��ư�� ��Ȱ��ȭ, Ÿ�� ��� �ܰ��� ȿ�� �ο�
            if (isTargeting)
                return;

            if (caster == null)
            {
                Debug.LogError($"MemberSkillSlotUI�� ���� ������ �������� �ʽ��ϴ�.");
                return;
            }

            if (members.Find(member => member == caster) == null)
            {
                Debug.LogError($"BattleManager�� MemberSkillSlotUI�� ������ �ִ� {caster.Name} ���ᰡ �������� �ʽ��ϴ�.");
                return;
            }

            if (index < 0 || index >= 4)
            {
                Debug.LogError($"{index}�� �������� �ʴ� ��ų �����Դϴ�.");
                return;
            }

            Skill skill = caster.SkillSlot[index];
            if (skill == null)
                return;

            targets.Clear();

            // Ÿ������ �ʿ��� ��ų�� ��� Ÿ���� ����
            if (skill.TargetingType == SkillTargetingType.ONE_ENEMY || skill.TargetingType == SkillTargetingType.ONE_PARTY)
            {
                isTargeting = true;
                this.caster = caster;
                castingSkill = skill;

                WaitTargeting(index, setCooltimeUI);
            }
            // Ÿ������ �ʿ����� ���� ��ų�� ��� �ٷ� ��ų ����
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

        #region ���� ���� �� ���� ����
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
