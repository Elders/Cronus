using Machine.Specifications;

namespace Elders.Cronus.Tests.PublisherTests;


[Subject("Publisher")]
public class When__negative
{
    Because of = () => result = negative;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult negative = new PublishResult(false, true);
}

[Subject("Publisher")]
public class When__negative1
{
    Because of = () => result = negative;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult negative = new PublishResult(false, false);
}

[Subject("Publisher")]
public class When__negative2
{
    Because of = () => result = negative;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult negative = new PublishResult(true, false);
}

[Subject("Publisher")]
public class When__positive
{
    Because of = () => result = positive;

    It should_be_positive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult positive = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__possitive2
{
    Because of = () => result = positive1 && positive2;

    It should_be_positive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult positive1 = new PublishResult(true, true);
    static PublishResult positive2 = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__possitive3
{
    Because of = () => result = positive && negative;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult positive = new PublishResult(true, true);
    static PublishResult negative = new PublishResult(false, false);
}

[Subject("Publisher")]
public class When__possitive4
{
    Because of = () => result = positive1 &= positive2;

    It should_be_positive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult positive1 = new PublishResult(true, true);
    static PublishResult positive2 = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__pos5
{
    Because of = () => result = positive &= negative;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult negative = new PublishResult();
    static PublishResult positive = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__possitive55
{
    Because of = () => result = negative &= positive;

    It should_be_possitive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult negative = new PublishResult();
    static PublishResult positive = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__possitive554
{
    Because of = () => result = start && negative1 && negative2 && positive1;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult start = new PublishResult();
    static PublishResult negative1 = new PublishResult(true, false);
    static PublishResult negative2 = new PublishResult(false, false);
    static PublishResult positive1 = new PublishResult(true, true);

}

[Subject("Publisher")]
public class When__possitive554235
{
    Because of = () => result = start && positive1 && negative && positive2;

    It should_be_negative = () => result.ShouldBeFalse();

    static bool result = false;
    static PublishResult start = new PublishResult();
    static PublishResult positive1 = new PublishResult(true, true);
    static PublishResult negative = new PublishResult(false, false);
    static PublishResult positive2 = new PublishResult(true, true);

}

[Subject("Publisher")]
public class When__possitive554stg
{
    Because of = () => result = start && negative1 && negative2 && positive3;

    It should_be_possitive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult start = new PublishResult();
    static PublishResult negative1 = new PublishResult(true, false);
    static PublishResult negative2 = new PublishResult(true, false);
    static PublishResult positive3 = new PublishResult(true, true);
}

[Subject("Publisher")]
public class When__possitive554stg235
{
    Because of = () => result = start && negative1 && negative2 && positive3;

    It should_be_possitive = () => result.ShouldBeTrue();

    static bool result = false;
    static PublishResult start = new PublishResult();
    static PublishResult negative1 = new PublishResult(true, false);
    static PublishResult negative2 = new PublishResult(true, false);
    static PublishResult positive3 = new PublishResult(true, true);
}
