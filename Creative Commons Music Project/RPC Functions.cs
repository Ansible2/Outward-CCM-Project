namespace creativeCommonsMusicProject
{
    public class CCM_rpc
    {
        CCM_base CCM_base = new CCM_base(); // how to get another class in a different file
        void myThing()
        {
            CCM_base.CCM_fnc_logWithTime();
        }
    }
}