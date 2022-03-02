using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UI_Chart.Views;

namespace UI_Chart {
    public class UI_ChartModule : IModule {
        public void OnInitialized(IContainerProvider containerProvider) {
            SciChart.Charting.Visuals.SciChartSurface.SetRuntimeLicenseKey(@"kYMxTdL/97LIJXDgdLGvX7NDXTOvOfRgSfFR4udlCnFlaxQKrJFEH/nC6bn1y/yiP9BN7t0QDK6JwtzEzgaJKxeZ42odavEsBf8KTepVNqr6sRF90BndZFRA/tU7icqx4++ehmws/yhpd3ZxIAwFxHhFjWo256vodulK8yUnDxdXHred1ikANquQElAGT2SCehxl+VncBVgFB/FGJkcUINp6vyTsPqsWjcjxRQqK2r9FX++spI8EYiHHIqKEls3xrLUS7uPOheXjmhnkmIB4gQzZFPWBugNTGO4ixgOTPnlMV5DWmMINYLptgZ9JQK04lGyL1C7p++oERMOIUX+aqVQDQtsMXZCcICmqbNwZ3HsttGEXgHGwbMKrb0L3U7+ftrRwNOMUxbqSnrf64F10TEcx6ghJiEEECgymOb0BewGjNq3gCf/DDeTChQwasLsOwdxcregZ/SiX2pyASP76jfjmzOZU/WX/xh+fF+kwBDjOhlskRndLdAUbwxpVBlj6Iv+wFFQynVofX8gXj9nwDycHOceHHRKktF487sz3/qJXvRmO1zItqA+/QLhbcHpdfxlRjR1JKlzMV0yOg+AJDdn0rSXeBHWWV3Oy7fQrmz2fEw==");

            var regionManager = containerProvider.Resolve<IRegionManager>();

        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<Trend>();
            containerRegistry.RegisterForNavigation<Summary>();
            containerRegistry.RegisterForNavigation<Raw>();
            containerRegistry.RegisterForNavigation<ItemCorr>();
            containerRegistry.RegisterForNavigation<WaferMap>();

        }
    }
}