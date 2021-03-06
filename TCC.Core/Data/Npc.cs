﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using TCC.ViewModels;


namespace TCC.Data
{
    public class Npc : TSPropertyChanged, IDisposable
    {
        public bool HasGage { get; set; }

        public ulong EntityId { get; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NPC(nameof(Name));
                }
            }
        }

        public bool IsBoss { get; set; }
        private SynchronizedObservableCollection<AbnormalityDuration> _buffs;
        public SynchronizedObservableCollection<AbnormalityDuration> Buffs
        {
            get => _buffs;
            set
            {
                if (_buffs == value) return;
                _buffs = value;
                NPC();
            }
        }

        private bool _enraged;
        public bool Enraged
        {
            get => _enraged;
            set
            {
                if (_enraged == value) return;
                _enraged = value;
                NPC();
            }
        }

        private float _maxHP;
        public float MaxHP
        {
            get => _maxHP;
            set
            {
                if (_maxHP != value)
                {
                    _maxHP = value;
                    EnragePattern?.Update(value);
                    NPC();
                }
            }
        }

        private float _currentHP;
        public float CurrentHP
        {
            get => _currentHP;
            set
            {
                if (_currentHP != value)
                {
                    _currentHP = value;
                    NPC(nameof(CurrentHP));
                    NPC(nameof(CurrentPercentage));
                    NPC(nameof(CurrentFactor));
                }
            }
        }
        private uint _maxShield;
        public uint MaxShield
        {
            get => _maxShield;
            set
            {
                if (_maxShield != value)
                {
                    _maxShield = value;
                    NPC(nameof(MaxShield));
                    NPC(nameof(ShieldFactor));
                }
            }
        }
        private float _currentShield;
        public float CurrentShield
        {
            get => _currentShield;
            set
            {
                if (_currentShield == value) return;
                _currentShield = value;
                NPC(nameof(CurrentShield));
                NPC(nameof(ShieldFactor));
            }
        }

        public double ShieldFactor => MaxShield > 0 ? CurrentShield / MaxShield : 0;

        public float CurrentFactor => _maxHP == 0 ? 0 : (_currentHP / _maxHP);
        public float CurrentPercentage => CurrentFactor * 100;
        private Visibility _visible;
        public Visibility Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;
                _visible = value;
                NPC();
            }
        }

        private ulong _target;
        public ulong Target
        {
            get => _target;
            set
            {
                if (_target == value) return;
                _target = value;
                NPC();
            }
        }

        private AggroCircle _currentAggroType = AggroCircle.None;
        public AggroCircle CurrentAggroType
        {
            get => _currentAggroType;
            set
            {
                if (_currentAggroType == value) return;
                _currentAggroType = value;
                NPC();
            }
        }

        public uint ZoneId { get; }
        public uint TemplateId { get; }

        public EnragePattern EnragePattern { get; set; }
        public TimerPattern TimerPattern { get; set; }

        public void AddorRefresh(Abnormality ab, uint duration, int stacks)
        {
            var existing = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Id);
            if (existing == null)
            {
                var newAb = new AbnormalityDuration(ab, duration, stacks, _target, Dispatcher, true);
                if (ab.Infinity) Buffs.Insert(0, newAb);
                else Buffs.Add(newAb);
                if (ab.IsShield)
                {
                    MaxShield = ab.ShieldSize;
                    CurrentShield = ab.ShieldSize;
                }
                return;
            }
            existing.Duration = duration;
            existing.DurationLeft = duration;
            existing.Stacks = stacks;
            existing.Refresh();

        }
        public void EndBuff(Abnormality ab)
        {
            try
            {
                var buff = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Id);
                if (buff == null) return;

                Buffs.Remove(buff);
                buff.Dispose();

                if (!ab.IsShield) return;
                CurrentShield = 0;
                MaxShield = 0;
            }
            catch
            {
                // ignored
            }
        }

        public bool IsTower => Utils.IsGuildTower(ZoneId, TemplateId);
        public bool IsPhase1Dragon => Utils.IsPhase1Dragon(ZoneId, TemplateId);

        public uint GuildId
        {
            get
            {
                BossGageWindowViewModel.Instance.GuildIds.TryGetValue(EntityId, out var val);
                return val;
            }
        }

        //public Npc(ulong eId, uint zId, uint tId, float curHP, float maxHP, Visibility visible)
        //{
        //    _dispatcher = BossGageWindowViewModel.Instance.GetDispatcher();
        //    EntityId = eId;
        //    Name = EntitiesManager.MonsterDatabase.GetName(tId, zId);
        //    ZoneId = zId;
        //    TemplateId = tId;
        //    MaxHP = maxHP;
        //    CurrentHP = curHP;
        //    _buffs = new SynchronizedObservableCollection<AbnormalityDuration>(_dispatcher);
        //    Visible = visible;

        //    IsShieldPhase = false;
        //    IsSelected = false;
        //    if (BossGageWindowViewModel.Instance.CurrentHHphase == HarrowholdPhase.Phase1)
        //    {
        //        ShieldDuration = new DispatcherTimer();
        //        ShieldDuration.Interval = TimeSpan.FromSeconds(13);
        //        ShieldDuration.Tick += ShieldFailed;
        //    }

        //}
        public Npc(ulong eId, uint zId, uint tId, bool boss, Visibility visible, EnragePattern ep = null, TimerPattern tp = null)
        {
            Dispatcher = BossGageWindowViewModel.Instance.GetDispatcher();
            EntityId = eId;
            Name = SessionManager.MonsterDatabase.GetName(tId, zId);
            MaxHP = SessionManager.MonsterDatabase.GetMaxHP(tId, zId);
            ZoneId = zId;
            IsBoss = boss;
            TemplateId = tId;
            CurrentHP = MaxHP;
            _buffs = new SynchronizedObservableCollection<AbnormalityDuration>(Dispatcher);
            Visible = visible;
            Shield = ShieldStatus.Off;
            IsSelected = true;
            EnragePattern = ep ?? new EnragePattern(10, 36);
            TimerPattern = tp;
            TimerPattern?.SetTarget(this);
            if (IsPhase1Dragon)
            {
                _shieldDuration = new Timer {Interval = BossGageWindowViewModel.Ph1ShieldDuration * 1000};
                _shieldDuration.Elapsed += ShieldFailed;
            }
        }

        public override string ToString()
        {
            return $"{EntityId} - {Name}";
        }

        public void Dispose()
        {
            foreach (var buff in _buffs) buff.Dispose();
            _shieldDuration?.Dispose();
            TimerPattern?.Dispose();
        }

        ///////////////////TIMER////////////////////////


        //////////////////SHIELD////////////////////////
        //TODO: make this a separate class
        private readonly Timer _shieldDuration;

        private ShieldStatus _shield;
        public ShieldStatus Shield
        {
            get => _shield;
            set
            {
                if (_shield == value) return;
                _shield = value;
                NPC(nameof(Shield));
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                NPC();
            }
        }
        private void ShieldFailed(object sender, EventArgs e)
        {
            _shieldDuration.Stop();
            Shield = ShieldStatus.Failed;
        }
        public void BreakShield()
        {
            _shieldDuration.Stop();
            Shield = ShieldStatus.Broken;
            Task.Delay(5000).ContinueWith(t =>
            {
                Shield = ShieldStatus.Off;
            });
        }
        public void StartShield()
        {
            _shieldDuration.Start();
            Shield = ShieldStatus.On;
        }

        public event Action DeleteEvent;
        public void Delete()
        {
            foreach (var buff in _buffs) buff.Dispose();

            DeleteEvent?.Invoke();
        }
    }

    public class TimerPattern : TSPropertyChanged, IDisposable
    {
        private readonly Timer _timer;
        protected bool Running => _timer.Enabled;
        protected Npc Target { get; set; }
        public int Duration { get; }

        public event Action Started;
        public event Action Ended;
        //public event Action Reset;

        public void Start()
        {
            _timer.Start();
            Started?.Invoke();
        }

        public virtual void SetTarget(Npc target)
        {
            Target = target;
        }

        public TimerPattern(int duration)
        {
            Duration = duration;
            _timer = new Timer(Duration*1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            Ended?.Invoke();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    public class HpTriggeredTimerPattern : TimerPattern
    {
        public float StartAt { get; }
        public HpTriggeredTimerPattern(int duration, float startAt) : base(duration)
        {
            StartAt = startAt;
        }

        public override void SetTarget(Npc target)
        {
            base.SetTarget(target);
            Target.PropertyChanged += OnTargetPropertyChanged;
        }

        private void OnTargetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Running) return;
            if (e.PropertyName != nameof(Npc.CurrentFactor)) return;

            if (Target.CurrentFactor < StartAt) Start();
        }
    }
}
