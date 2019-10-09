using NUnit.Framework;
using NullRef = System.NullReferenceException;
using Ex      = System.Exception;
using InvOp   = System.InvalidOperationException;
using static Activ.GOAP.Solver<Activ.GOAP.Agent>;

namespace Activ.GOAP{
public class SolverTest : TestBase{

    Solver<Agent> x;
    Goal<Agent> unreachable = new Goal<Agent>(x => false),
                stasis      = new Goal<Agent>(x => true),
                hStasis     = new Goal<Agent>(x => true, x => 0f);

    [SetUp] public void Setup(){
        x = new Solver<Agent>();
        x.maxIter = x.maxNodes = 100;
    }

    // isRunning ---------------------------------------------------

    [Test] public void IsRunning_false_on_construct()
    => o( x.isRunning, false );

    [Test] public void IsRunning_false_after_solving(){
        var g = new Goal<Agent>(x => true);
        var z = x.Next(new Idler(),
                       in g);
        o( x.isRunning, false);
    }

    [Test] public void IsRunning_true_with_zero_frames_budget(){
        var z = x.Next(new Idler(), unreachable, iter: 0);
        o( x.isRunning, true);
    }

    [Test] public void IsRunning_true_with_remaining_frames(){
        var z = x.Next(new Inc(), unreachable, iter: 1);
        o( x.isRunning, true);
    }

    // Next --------------------------------------------------------

    [Test] public void Next_no_agent_throws()
    => Assert.Throws<NullRef>(
        () => x.Next((Agent)null, unreachable) );

    [Test] public void Next_start_at_goal()
    => o ( (string)x.Next(new Idler(), stasis), INIT );

    [Test] public void Next_start_at_goal_with_heuristic()
    => o ( (string)x.Next( new Idler(),
                   hStasis), INIT );

    [Test] public void Next_OTPoneyPassThrough()
    => o( (string)x.Next(new OTPoney(), stasis), INIT );

    [Test] public void Next_HeuristicOTPoneyPassThrough()
    => o( (string)x.Next(new OTPoney(), hStasis), INIT );

    [Test] public void Next_use_heuristic(){
        bool h = false;
        x.Next(new Inc(), new Goal<Agent>(
            x => false, x => { h = true; return 0f; }));
        o( h, true );
    }

    [Test] public void IgnoreHeuristic(){
        bool h = false; x.brfs = true;
        x.Next(new Inc(), new Goal<Agent>(
            x => false, x => { h = true; return 0f; }));
        o( h, false );
    }

    // Iterate -----------------------------------------------------

    [Test] public void Iterate_no_init_state()
    => Assert.Throws<InvOp>( () => x.Iterate() );

    [Test] public void Iterate_stalled(){
        x.maxIter = 2;
        x.Next(new Inc(), unreachable, 10);
        o(x.state == PlanningState.Stalled);
        var z = x.Iterate();
        o(z, null);
    }

    [Test] public void Iterate_no_solution(){
        x.Next(new SixShot(), unreachable, 4);
        o(x.state != PlanningState.Stalled);
        var z = x.Iterate(4);
        o(z, null);
        o(x.state, PlanningState.Failed);
    }

    [Test] public void Iterate_stalling(){
        x.maxIter = 8;
        x.Next(new Inc(), unreachable, 5);
        o(x.state != PlanningState.Stalled);
        var z = x.Iterate(5);
        o(x.state == PlanningState.Stalled);
    }

    // Expansions --------------------------------------------------

    [Test] public void ExpandActions_zero_cost_throws()
    => Assert.Throws<Ex>(
            () => x.Next(new CouchSurfer(), unreachable) );

    [Test] public void ExpandMethods_zero_cost_throws()
    => Assert.Throws<Ex>(
            () => x.Next(new Freeloader(), unreachable) );

    [Test] public void ExpandMethods_zero_cost_brfs(){
        x.brfs = true;
        var s = x.Next(new Freeloader(), unreachable);
    }

}}
