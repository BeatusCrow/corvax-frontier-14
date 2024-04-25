using Content.Shared.Interaction;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Content.Shared.Tools.Components;
using Content.Shared.Item;
using Robust.Shared.Random;

namespace Content.Server.Weapons.Melee.EnergySword
{
    public sealed class EnergySwordSystem : EntitySystem
    {
        [Dependency] private readonly SharedRgbLightControllerSystem _rgbSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EnergySwordComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<EnergySwordComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<EnergySwordComponent, ItemUnwieldedEvent>(TurnOffonUnwielded);
            SubscribeLocalEvent<EnergySwordComponent, ItemWieldedEvent>(TurnOnonWielded);
        }

        // Used to pick a random color for the blade on map init.
        private void OnMapInit(EntityUid uid, EnergySwordComponent comp, MapInitEvent args)
        {
            if (comp.ColorOptions.Count != 0)
                comp.ActivatedColor = _random.Pick(comp.ColorOptions);

            if (!TryComp(uid, out AppearanceComponent? appearanceComponent))
                return;
            _appearance.SetData(uid, ToggleableLightVisuals.Color, comp.ActivatedColor, appearanceComponent);
        }

        // Used to make the make the blade multicolored when using a multitool on it.
        private void OnInteractUsing(EntityUid uid, EnergySwordComponent comp, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (!TryComp(args.Used, out ToolComponent? tool) || !tool.Qualities.ContainsAny("Pulsing"))
                return;

            args.Handled = true;
            comp.Hacked = !comp.Hacked;

            if (comp.Hacked)
            {
                var rgb = EnsureComp<RgbLightControllerComponent>(uid);
                _rgbSystem.SetCycleRate(uid, comp.CycleRate, rgb);
            }
            else
                RemComp<RgbLightControllerComponent>(uid);
        }

        private void TurnOffonUnwielded(EntityUid uid, EnergySwordComponent comp, ItemUnwieldedEvent args)
        {
            var ev = new EnergySwordDeactivatedEvent();
            RaiseLocalEvent(uid, ref ev);
        }

        private void TurnOnonWielded(EntityUid uid, EnergySwordComponent comp, ref ItemWieldedEvent args)
        {
            var ev = new EnergySwordActivatedEvent();
            RaiseLocalEvent(uid, ref ev);
        }
    }
}
