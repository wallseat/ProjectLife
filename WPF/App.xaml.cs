using System;
using System.Windows;
using System.Windows.Threading;
using ProjectLife_v_0_3.ProjectLife;

namespace ProjectLife_v_0_3.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _wnd;
        private DispatcherTimer _timer;

        private World _world;
        private Drawer _drawer;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            _world = new World(120, 80);
            _drawer = new Drawer(_world, 6);
            _wnd = new MainWindow(_drawer, _world);
            _timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1)};

            _timer.Tick += (o, args) =>
            {
                if (!_wnd.Paused)
                {
                    for (int i = 0; i < _wnd.UpdatesPerFrame.Value; i++)
                        _world.Update();
                }

                _wnd.Generation.Text = _world.Generation.ToString();
                _wnd.MineralsCount.Text = _world.MineralsCount.ToString();
                _wnd.OrganicCount.Text = _world.OrganicCount.ToString();
                _wnd.LifeCount.Text = _world.LifeCount.ToString();

                _drawer.DrawWorld();
            };

            _timer.Start();

            _wnd.Show();
        }
    }
}