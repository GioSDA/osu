// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItem : OsuClickableContainer
    {
        public event Action<Channel>? OnRequestSelect;
        public event Action<Channel>? OnRequestLeave;

        public readonly BindableInt Mentions = new BindableInt();

        public readonly BindableBool Unread = new BindableBool();

        private readonly Channel channel;

        private Box? hoverBox;
        private Box? selectBox;
        private OsuSpriteText? text;
        private ControlItemClose? close;

        [Resolved]
        private Bindable<Channel> selectedChannel { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public ControlItem(Channel channel)
        {
            this.channel = channel;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = 30;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                    Alpha = 0f,
                },
                selectBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                    Alpha = 0f,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 18, Right = 10 },
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                createIcon(),
                                text = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = channel.Type == ChannelType.Public ? $"# {channel.Name.Substring(1)}" : channel.Name,
                                    Font = OsuFont.Torus.With(size: 17, weight: FontWeight.SemiBold),
                                    Colour = colourProvider.Light3,
                                    Margin = new MarginPadding { Bottom = 2 },
                                    RelativeSizeAxes = Axes.X,
                                    Truncate = true,
                                },
                                new ControlItemMention
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding { Right = 3 },
                                    Mentions = { BindTarget = Mentions },
                                },
                                close = new ControlItemClose
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding { Right = 3 },
                                    Action = () => OnRequestLeave?.Invoke(channel),
                                }
                            }
                        },
                    },
                },
            };

            Action = () => OnRequestSelect?.Invoke(channel);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedChannel.BindValueChanged(change =>
            {
                if (change.NewValue == channel)
                    selectBox?.Show();
                else
                    selectBox?.Hide();
            }, true);

            Unread.BindValueChanged(change =>
            {
                text!.Colour = change.NewValue ? colourProvider.Content1 : colourProvider.Light3;
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox?.Show();
            close?.Show();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox?.Hide();
            close?.Hide();
            base.OnHoverLost(e);
        }

        private Drawable createIcon()
        {
            if (channel.Type != ChannelType.PM)
                return Drawable.Empty();

            return new ControlItemAvatar(channel)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };
        }
    }
}
