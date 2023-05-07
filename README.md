# BICGame

## 1. 프로젝트 Assets 구조

```
ASSETS  
├─Animations : 애니메이션 폴더
├─Dialogue System : 대화 편집 시스템 관련 폴더
│  ├─Dialogues : 대화 데이터 저장 폴더
├─Editor : 유니티 커스텀 에디터 추가 폴더
│  └─Dialogue System : 대화 편집 시스템 에디터 관련 폴더
│      ├─Graphs : 대화 편집 시스템 화면 구성 데이터 저장 폴더
├─Editor Default Resources : 유니티 커스텀 에디터 uss 저장 폴더
├─Etc : 기타 저장 폴더
├─Externals : 외부 에셋 저장 폴더
│  └─Something Demo : 현재 라이센스 확인 필요
├─Fonts : 폰트 폴더
├─Map : Map 폴더
├─Materials : Material 폴더
├─Prefabs : Prefab 폴더
├─Scenes : Scene 폴더
├─Scripts : c# 소스코드 폴더
├─Sprites : Sprite 폴더
└─TextMesh Pro : TextMesh Pro 라이브러리 폴더
```

## 2. 대화 편집 시스템 사용 방법

1. *`Window > DS > Dialogue Graph`* 클릭

    ![image](https://user-images.githubusercontent.com/44758316/236600715-7a5b2834-7dad-4b5a-a1c4-a1d675c25cbf.png)

2. 노드 및 그룹 생성 메뉴

    Dialogue Graph 편집 창에서 *`마우스 우클릭 > 원하는 메뉴`* 클릭

    ![image](https://user-images.githubusercontent.com/44758316/236600944-dc01942e-6399-4c09-9eb1-f172e7d2270f.png)
    
    * Create Node (단축키 스페이스바) : 검색 윈도우를 띄워 메뉴 선택
    
        ![image](https://user-images.githubusercontent.com/44758316/236600993-fb76221e-acde-40f1-aeed-20b8f700b1d2.png)
        ![image](https://user-images.githubusercontent.com/44758316/236601000-922750f1-c170-42cb-9f79-50639dd7b343.png)

    * Add Node (Single Choice) : 선택지가 없는 대사. 다음 대사로 넘어가거나 끝맺음만 가능.
    
        ![image](https://user-images.githubusercontent.com/44758316/236601386-1d834f95-b192-4496-8753-f61eeabf3ef0.png)

    * Add Node (Multiple Choice) : 선택지가 있는 대사. 선택지에 따른 다음 대사 구성 가능.

        ![image](https://user-images.githubusercontent.com/44758316/236601697-06ec5d15-055c-45c3-ac4d-63198a75a41d.png)

    * Add Group : 서로 관련이 있는 대사를 묶을 수 있는 그룹 생성
    
        ![image](https://user-images.githubusercontent.com/44758316/236601766-87d538ad-c63f-4497-9d5d-86852e0972d1.png)

3. 기타 조작 방법

    * 화면 조작
        1. 드래그 : 화면 이동
        2. 마우스 휠 : 화면 줌 인-아웃
        3. 편집 창에 존재하는 모든 노드를 보이게 화면 조정하기 : 단축키 `F`   
        4. 상단 버튼
        
            ![image](https://user-images.githubusercontent.com/44758316/236601997-9adbadd5-b49a-4329-9d8a-1da92c58416b.png)
            
            차례로 편집 내용 저장, 편집 내용 로드, 편집 내용 모두 지우기, 새로운 편집 내용 띄우기, 미니맵 토글


    * 기존에 존재하는 노드를 그룹에 넣기
        1. 기존에 생성되어 있는 그룹에 드래그 앤 드랍
        2. 노드 드래그 선택 후 우클릭 메뉴로 그룹 생성
        
            ![image](https://user-images.githubusercontent.com/44758316/236602086-995a3b1e-497a-4261-be61-9ca7d053b97a.png)
            ![image](https://user-images.githubusercontent.com/44758316/236602109-06cf0f8a-0641-49d6-91c2-37dc411c9711.png)

    * 대사 연결 제거
        1. 해당 연결 클릭 후 *`Delete`*
        2. 해당 노드 우클릭 후 *`Disconnect Input Ports`*, *`Disconnect Output Ports`*, *`Disconnect All`* 클릭
        
            ![image](https://user-images.githubusercontent.com/44758316/236602195-7f10eb8a-9375-4421-b5bd-a40b79e4b6ad.png)

    * 노드 또는 그룹 제거
        1. 해당 노드 또는 그룹 클릭 후 *`Delete`*
        2. 해당 노드 또는 그룹 우클릭 후 *`Delete`* 클릭
        3. 해당 노드 또는 그룹 드래그 선택 후 *`Delete`*
