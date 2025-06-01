using Silk.NET.Maths;

namespace TheAdventure.Models;

public class EnemyObject : RenderableGameObject
{
    private const int _speed = 64; // pixels per second, mai lent decât player-ul
    private const int _detectionRadius = 200; // raza în care inamicul detectează player-ul
    private const int _attackRadius = 32; // raza în care inamicul poate ataca
    private const int _damage = 10; // damage-ul făcut player-ului
    private DateTimeOffset _lastAttackTime = DateTimeOffset.Now;
    private const double _attackCooldown = 1000; // 1 secundă între atacuri

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    public EnemyState State { get; private set; }

    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        State = EnemyState.Idle;
        SpriteSheet.ActivateAnimation("Idle");
    }

    public void Update(PlayerObject player, double time)
    {
        if (player == null) return;

        var distanceToPlayer = CalculateDistanceToPlayer(player);
        
        if (distanceToPlayer <= _detectionRadius)
        {
            if (State != EnemyState.Chase)
            {
                State = EnemyState.Chase;
                SpriteSheet.ActivateAnimation("Chase");
            }
            ChasePlayer(player, time);
        }
        else
        {
            if (State != EnemyState.Idle)
            {
                State = EnemyState.Idle;
                SpriteSheet.ActivateAnimation("Idle");
            }
        }

        if (distanceToPlayer <= _attackRadius)
        {
            TryAttack(player);
        }
    }

    private double CalculateDistanceToPlayer(PlayerObject player)
    {
        var dx = Position.X - player.Position.X;
        var dy = Position.Y - player.Position.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private void ChasePlayer(PlayerObject player, double time)
    {
        var pixelsToMove = _speed * (time / 1000.0);
        
        double dx = player.Position.X - Position.X;
        double dy = player.Position.Y - Position.Y;
        
        var length = Math.Sqrt(dx * dx + dy * dy);
        if (length > 0)
        {
            dx /= length;
            dy /= length;
        }

        Position = (
            Position.X + (int)(dx * pixelsToMove),
            Position.Y + (int)(dy * pixelsToMove)
        );
    }

    private void TryAttack(PlayerObject player)
    {
        var timeSinceLastAttack = (DateTimeOffset.Now - _lastAttackTime).TotalMilliseconds;
        if (timeSinceLastAttack >= _attackCooldown)
        {
            player.TakeDamage(_damage);
            _lastAttackTime = DateTimeOffset.Now;
            State = EnemyState.Attack;
            SpriteSheet.ActivateAnimation("Attack");
        }
    }

    public override void Render(GameRenderer renderer)
    {
        if (SpriteSheet.ActiveAnimation == null)
        {
            SpriteSheet.ActivateAnimation("Idle");
        }
        base.Render(renderer);
    }
} 