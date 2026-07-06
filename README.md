# Memorix

게임과 연동되는 AI 기반 초개인화 영단어 학습 플랫폼의 Unity 게임 클라이언트입니다. Anki·Quizlet·Duolingo류 앱의 "학습 동기가 전적으로 의지에 의존한다"는 한계를 "학습 → 게임 → 재미 → 학습" 선순환 구조로 풀어냈습니다. 학습 세션 성과가 로그라이트 게임 캐릭터의 성장으로 이어지고, 게임 진행이 막히면 다시 학습으로 돌아오게 됩니다. CAT·FSRS·IRT 알고리즘과 Claude API가 통합된 백엔드(FastAPI·PostgreSQL, 팀원 개발·별도 레포)와 연동해 동작합니다.

- 개발 기간: 2026.03 ~ 2026.06 (산학캡스톤디자인, 약 3개월)
- 개발 인원: 3인 팀 (기획 / **Unity 개발** / 서버 개발) — 본 레포는 Unity 클라이언트 전체(게임 시스템·UI·백엔드 API 연동) 담당분
- 빌드 대상: Android (`com.Memorix.Memorix`)

**Youtube Link**  
[![Video Label](http://img.youtube.com/vi/dmcUIaUCXAY/0.jpg)](https://youtu.be/dmcUIaUCXAY?t=0s)

## 기술 스택

| 구분 | 내용 |
|---|---|
| 엔진 / 렌더링 | Unity 6000.3.16f1 (Unity 6), Universal Render Pipeline 17.3.0 |
| 언어 | C# |
| 주요 패키지 | Input System 1.19.0, com.h8man.2d.navmeshplus (2D NavMesh) |
| 통신 | UnityWebRequest 기반 REST/JSON (백엔드 17개 엔드포인트 연동) |

## 핵심 구현

### 1. 데이터 주도 스킬 · 스탯 시스템 (전략 패턴)

스킬 효과와 스탯 성장 공식을 각각 `SkillEffect`, `ScalingFormula` 추상 클래스로 정의하고, 실제 밸런싱 데이터는 코드가 아닌 ScriptableObject 애셋으로 분리했습니다. 새 스킬/공식을 추가할 때 기존 코드를 건드리지 않고 애셋만 새로 만들면 되도록 구현했습니다.

```csharp
public abstract class ScalingFormula : ScriptableObject
{
    // 1.0 = 원본 그대로, 2.0 = 2배
    public abstract float Evaluate(int stage, int totalStages);
}

[CreateAssetMenu(menuName = "Stat Scaling/S-Curve")]
public class SCurveFormula : ScalingFormula
{
    public float minMultiplier = 1f, maxMultiplier = 4f;

    public override float Evaluate(int stage, int totalStages)
    {
        float t = Mathf.Clamp01((float)stage / totalStages);
        float smoothT = t * t * (3f - 2f * t); // smoothstep
        return Mathf.Lerp(minMultiplier, maxMultiplier, smoothT);
    }
}
```

스탯 성장 공식 **5종**(선형·지수·S커브·계단식·수동), 스킬 이펙트 **15종**(다중샷·관통·반사·도탄 등)을 `SkillTriggerType`(Passive/OnAttack/OnDamaged/OnHit/OnKill/TimeBased 등)에 따라 데이터로 관리합니다. 캡스톤 특성상 개발 후반까지 밸런싱 수치가 자주 바뀔 것으로 예상해, 값이 바뀔 때마다 코드를 재빌드하지 않고 애셋만 교체할 수 있도록 처음부터 이 구조로 설계했습니다. 실제로 개발 중반 원래 플레이어 전용이던 공격 로직(`AttackHelper`)을 몬스터도 재사용할 수 있게 `SkillEffect` 체계로 통합하는 리팩토링을 거쳤는데, 이 구조 덕분에 몬스터 스킬을 새 코드 없이 `DefaultShotSkillEffect` 애셋 하나로 추가할 수 있었습니다.

### 2. 자체 구현 Behaviour Tree — 텔레그래프 공격 패턴

에셋스토어 BT 대신 Selector/Sequence/Decorator(Condition·Cooldown·Invert·Repeat 등)를 직접 구현했습니다. 아래는 실제 몬스터 AI 구성 예시로, 쿨타임마다 스킬을 시전하되 시전 전 `DangerTrail`(공격 예고선) 오브젝트를 미리 생성해 플레이어에게 회피 타이밍을 주는 텔레그래프 패턴입니다.

```csharp
new BTCooldownDecorator(new BTSequenceNode(new List<BTNode>
{
    new BTCheckPlayerIsInRange(monster, 7.5f, true),
    new BTMoveStop(monster),
    new BTInvoke(PrepareSkill)   // 예고선(DangerTrail) 표시 후 지연 공격
}), attackDelay);
```

NavMesh2D·Animator와 세밀하게 엮인 몬스터별 행동(예고 공격, 애니메이션 이벤트 연동)을 만들어야 했기 때문에, 기성 에셋보다 직접 구현이 수정·확장에 유리하다고 판단했습니다. 여기에 BT의 동작 원리를 직접 익히려는 학습 목적도 있었습니다.

### 3. 오브젝트 풀링

탄막·이펙트가 잦은 전투 특성상 `Instantiate`/`Destroy` 호출을 큐 기반 풀로 대체했습니다. 풀이 비어 있으면 자동으로 확장되는 지연 초기화(lazy) 방식입니다.

```csharp
public IPool PoolingObj(string path)
{
    if (!m_poolDictionary.ContainsKey(path)) AddPool(path);
    if (m_poolDictionary[path].Pool.Count <= 0) AddQueue(path); // 부족하면 자동 확장
    return m_poolDictionary[path];
}
```

투사체·이펙트 생성량이 로그라이트 전투 특성상 많을 것으로 예상해 선제적으로 도입했습니다. (Profiler로 GC Alloc을 실측 비교하진 않았습니다.)

### 4. 결정론적(Seed 기반) 난수

FNV-1a 해시로 시드를 만들고 xorshift 유사 믹싱을 거쳐, 같은 인덱스·시드 조합이면 항상 같은 결과를 내는 난수를 구현했습니다.

```csharp
public static float RandomFromIndex(int index, string seed)
{
    uint x = (uint)index + HashString(seed); // FNV-1a
    x ^= x >> 16; x *= 0x7feb352d;
    x ^= x >> 15; x *= 0x846ca68b;
    x ^= x >> 16;
    return (x & 0xFFFFFF) / (float)0xFFFFFF;
}
```

스테이지 노드 배치가 플레이어마다 매번 랜덤하게 뒤바뀌지 않고, 같은 조건(시드)이면 항상 같은 배치로 재현되어야 했기 때문에 `System.Random` 대신 인덱스·시드 기반의 결정론적 난수를 직접 구현했습니다.

### 5. 백엔드 API 연동 — 가변 응답 스키마 처리

`ApiManager`(총 385줄, CAT 온보딩·일일 스케줄·세션 결과 제출 등 10여 개 엔드포인트)는 제네릭 GET/POST 래퍼로 타임아웃 설정과 에러 파싱을 한 곳에 모아 호출부 중복을 없앴습니다.

가장 까다로웠던 부분은 CAT 온보딩 답변 제출 엔드포인트(`/api/onboarding/cat/answer`)였습니다. 이 API는 같은 엔드포인트인데도 온보딩이 "진행 중"이면 다음 문항(`CatQuestion`), "완료"면 유저 프로필(`CatResult`)로 서로 다른 스키마를 반환합니다. Unity의 `JsonUtility`는 이런 유니언 타입을 직접 지원하지 않기 때문에, 두 응답에 공통으로 존재하는 `done` 필드만 먼저 걸쳐 파싱(peek)한 뒤 값에 따라 실제 타입으로 다시 파싱하는 방식으로 처리했습니다.

```csharp
// 공통 필드(done)만 먼저 peek
var peek = JsonUtility.FromJson<CatResult>(raw);
if (peek.done)
{
    onDone?.Invoke(peek);           // 완료: 유저 프로필로 재파싱
}
else
{
    var q = JsonUtility.FromJson<CatQuestion>(raw);
    onQuestion?.Invoke(q);          // 진행 중: 다음 문항으로 재파싱
}
```

서버 에러 응답도 `{detail, status_code}` 구조화된 JSON으로 우선 파싱을 시도하고, 실패하면 원본 에러 메시지로 폴백하도록 처리했습니다.

## 트러블슈팅

### 공격 로직 중복 → 스킬 시스템으로 통합

개발 초반에는 플레이어 공격(`AttackHelper` + 자체 투사체 스폰 로직)과 몬스터 공격(몬스터 전용 `DefaultMonsterAttackObjectSpawner` 클래스)이 완전히 분리된 코드로 존재해, 몬스터 공격 패턴을 하나 추가할 때마다 전용 스포너 클래스를 새로 작성해야 했습니다. 몬스터 프리팹에서 전용 스포너 컴포넌트를 제거하고, 플레이어와 동일한 `SkillManager` + `SkillEffect`(ScriptableObject) 조합으로 교체해 `DefaultShotSkillEffect`라는 범용 스킬 이펙트 애셋 하나로 몬스터도 플레이어와 같은 투사체 스폰 파이프라인(`ShotSkillEffect.SpawnProjectile`)을 재사용하도록 정리했습니다. 이후 몬스터 스킬은 코드 작성 없이 애셋 조합만으로 추가할 수 있게 됐고, [핵심 구현 1번](#1-데이터-주도-스킬--스탯-시스템-전략-패턴)의 데이터 주도 스킬 시스템이 플레이어/몬스터 구분 없이 하나의 체계로 통합된 계기입니다.

### 공격 예고선과 실제 공격 방향 불일치

몬스터가 스킬을 준비할 때 `DangerTrail`(공격 예고선)이 가리키는 방향과 실제로 투사체가 발사되는 방향이 어긋나는 버그가 있었습니다. 공격 방향(`플레이어 위치 - 몬스터 위치`)을 예고선을 표시하는 시점과 실제 공격을 실행하는 시점, 두 곳에서 각각 따로 계산하고 있었기 때문에, 몬스터가 스킬을 준비하는 딜레이 동안 플레이어가 이동하면 두 시점의 방향 값이 서로 달라졌습니다.

```csharp
// PrepareSkillCoroutine — 예고선을 띄우는 시점에 방향을 한 번만 계산해 캐싱
AttackDirection = (Player.Instance.transform.position - transform.position).normalized;

// Monster.Attack — 실제 발사 시에도 같은 값을 재사용 (재계산하지 않음)
Vector2 direction = (monsterBT as DefaultMonsterBT).AttackDirection;
```

방향을 예고선을 표시하는 시점에 한 번만 계산해 `AttackDirection` 프로퍼티에 캐싱하고, 실제 공격 시에는 이 값을 그대로 재사용하도록 수정해 해결했습니다. 같은 값을 두 지점에서 각각 계산하지 않고, 한 번 계산해 상태로 공유하도록 바꾼 전형적인 동기화 버그 수정 사례입니다.

### 피격 시 스킬 시전이 의도치 않게 취소되는 문제

몬스터가 스킬을 준비(공격 애니메이션의 시작과 공격 함수를 실행하는 키 프레임 사이)하는 동안 플레이어에게 공격받으면 그 즉시 스킬 시전이 취소되는 문제가 있었습니다. BT 트리에서 "피격 애니메이션(DAMAGED)이 재생 중인지"를 스킬 취소 조건으로 직접 사용하고 있었던 탓에, 애니메이션 상태(연출)와 스킬 시전 상태(게임 로직)가 뒤섞여 피격 연출이 재생되는 것만으로도 로직상 스킬이 취소됐습니다. `MonsterBT`에 `AttackAnimation()`/`GetDamagedAnimation()`/`DieAnimation()`과 같은 추상 메서드를 추가해 애니메이션 트리거 책임을 개별 몬스터 BT로 옮기고, 스킬 준비 중에는 피격 애니메이션 자체가 재생되지 않도록 조건을 바꾸어 애니메이션 상태와 게임 로직 상태를 분리했습니다.

## 회고

- **가장 많이 배운 것:** 투사체 반사·도탄 처리(`AttackProjectile.cs`)에서는 `Vector2.Reflect`로 반사 법칙을 구현하고, FixedUpdate의 실행 간격 문제로 Raycast로 얻은 벽 노멀이 없을 때를 대비한 폴백 체인, `Physics2D.OverlapCircleAll` + `sqrMagnitude` 비교로 가장 가까운 미피격 대상을 찾는 등 실제 벡터·충돌 수학을 다뤘습니다. 자체 구현 Behaviour Tree는 Selector/Sequence/Decorator를 조합해 몬스터별 행동을 트리로 구성하는 CS적 설계 감각을 익힐 수 있었고, 데이터 주도 스킬/스탯 전략 패턴은 코드와 밸런싱 데이터를 분리하는 설계 습관을 들이는 데 도움이 됐습니다.
- **다음에 다르게 할 것:** 자동화 테스트가 전혀 없다는 점이 가장 아쉽습니다. Unity Test Framework의 사용에 익숙하지 않아 실제로 써보지 못했고, 대신 수동 테스트 매니저(`TestQuizManager`)로 API 흐름을 눈으로 확인하는 방식에 의존했습니다. PlayMode/EditMode 테스트 작성법 자체를 별도로 공부할 필요가 있다고 느꼈습니다.
- **개선 계획:** 전투·스킬 핵심 로직(스킬 이펙트 실행, 데미지 계산 등)부터 PlayMode 자동화 테스트를 도입하고, `MapManager.Update()`에서 매 프레임 폴링으로 처리하던 스테이지 클리어 감지를 몬스터 사망 이벤트 기반으로 전환할 계획입니다.
