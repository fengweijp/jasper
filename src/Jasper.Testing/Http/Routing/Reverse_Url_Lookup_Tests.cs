﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper.Http.Routing;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.Routing
{
    public class Reverse_Url_Lookup_Tests
    {
        private static readonly UrlGraph urls = new UrlGraph();

        static Reverse_Url_Lookup_Tests()
        {
            readType<OneController>();
            readType<TwoController>();
            readType<QueryStringTestController>();
            readType<OnlyOneActionController>();
        }

        private static void readType<T>()
        {
            typeof (T).GetMethods().Where(x => x.DeclaringType != typeof(object)).Each(method =>
            {
                var route = RouteBuilder.Build(typeof (T), method);
                urls.Register(route);
            });
        }


        [Fact]
        public void find_by_handler_type_if_only_one_method()
        {
            urls.UrlFor<OnlyOneActionController>()
                .ShouldBe("/go");
        }

        [Fact]
        public void retrieve_by_controller_action_even_if_it_has_an_input_model()
        {
            urls.UrlFor<OneController>(x => x.get_one_M1(null), null).ShouldBe("/one/m1");
        }

        [Fact]
        public void retrieve_a_url_for_a_model_simple_case()
        {
            urls.UrlFor(new Model1()).ShouldBe("/one/m1");
        }

        [Fact]
        public void retrieve_a_url_for_a_inferred_model_simple_case()
        {
            urls.UrlFor<Model1>((string)null).ShouldBe("/one/m1");
        }

        [Fact]
        public void retrieve_a_url_for_a_model_that_does_not_exist()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() => { urls.UrlFor(new ModelWithNoChain()); });
        }


        [Fact]
        public void retrieve_a_url_for_a_model_that_has_route_inputs()
        {
            urls.UrlFor(new ModelWithInputs
            {
                Name = "Jeremy"
            }).ShouldBe("/find/Jeremy");
        }
        /*
        [Fact]
        public void retrieve_a_url_for_a_model_that_has_querystring_inputs()
        {
            var model = new ModelWithQueryStringInput() { Param = 42 };

            urls.UrlFor(model).ShouldBe("/qs/test?Param=42");
        }
        */
        /*
        [Fact]
        public void retrieve_a_url_for_a_model_that_has_mixed_inputs()
        {
            var model = new ModelWithQueryStringAndRouteInput() { Param = 42, RouteParam = 23 };

            urls.UrlFor(model).ShouldBe("/qsandroute/test/23?Param=42");
        }
        */

        [Fact]
        public void retrieve_url_by_urn_name_with_parameters()
        {
            var dict = new Dictionary<string, object> { {"Name", "Max"} };

            urls.UrlFor("find_by_name", dict).ShouldBe("/find/Max");
        }

        [Fact]
        public void find_route_by_name_positive()
        {
            urls.UrlFor("A").ShouldBe("/one/a");
        }


        [Fact]
        public void retrieve_a_url_for_a_model_and_http_method()
        {
            urls.UrlFor(new UrlModel(), "GET").ShouldBe("/urlmodel");
            urls.UrlFor(new UrlModel(), "POST").ShouldBe("/urlmodel");
        }

        [Fact]
        public void retrieve_by_model_with_multiples()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() =>
            {
                urls.UrlFor(new UrlModel());
            });
        }

        [Fact]
        public void retrieve_a_url_by_action()
        {
            urls.UrlFor<OneController>(x => x.delete_one_m2(), null).ShouldBe("/one/m2");
        }

        [Fact]
        public void retrieve_a_url_by_action_negative_case()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() =>
            {
                urls.UrlFor<RandomClass>(x => x.Ignored(), null);
            });
        }

        [Fact]
        public void url_for_handler_type_and_method_positive()
        {
            var method = ReflectionHelper.GetMethod<OneController>(x => x.head_one_m3());

            urls.UrlFor(typeof(OneController), method).ShouldBe("/one/m3");
        }

        [Fact]
        public void url_for_handler_type_and_method_positive_by_method_name()
        {
            urls.UrlFor(typeof(OneController), nameof(OneController.head_one_m3)).ShouldBe("/one/m3");
        }

        [Fact]
        public void url_for_handler_type_and_method_negative_case_should_throw_204()
        {
            Exception<UrlResolutionException>.ShouldBeThrownBy(() => {
                var method = ReflectionHelper.GetMethod<RandomClass>(x => x.Ignored());
                urls.UrlFor(typeof(OneController), method, null);
            });
        }


        [Fact]
        public void url_for_by_type_respects_the_absolute_path()
        {
            urls.UrlFor<Model6>()
                .ShouldBe("/one/a");
        }



    }


    public class RandomClass
    {
        public void Ignored()
        {
        }
    }

    public class OneController
    {
        [RouteName("find_by_name")]
        public void get_find_Name(ModelWithInputs input)
        {
        }

        [RouteName("A")]
        public void get_one_a(Model6 input)
        {
        }

        public void B(Model7 input)
        {
        }


        public void get_one_M1(Model1 input)
        {
        }

        public void delete_one_m2()
        {
        }

        public void head_one_m3()
        {
        }

        public void M5(Model3 input)
        {
        }


        public string Default(DefaultModel model)
        {
            return "welcome to the default view";
        }
    }

    public class TwoController
    {
        public void M1()
        {
        }

        public void M2()
        {
        }

        public void M3()
        {
        }

        public void get_urlmodel(UrlModel input)
        {
        }

        public void post_urlmodel(UrlModel input) { }
    }

    public class OnlyOneActionController
    {
        public void Go(Model8 input)
        {
        }
    }

    public class QueryStringTestController
    {
        public void get_qs_test(ModelWithQueryStringInput input)
        {
        }

        public void get_qsandroute_test_RouteParam(ModelWithQueryStringAndRouteInput input)
        {
        }
    }

    public class ModelWithInputs
    {
        public string Name { get; set; }
    }

    public class Model1
    {
    }

    public class Model2
    {
    }

    public class Model3
    {
    }

    public class Model4
    {
    }

    public class Model5
    {
    }

    public class Model6
    {
    }

    public class Model7
    {
    }

    public class Model8
    {
    }

    public class DefaultModel
    {
    }

    public class ModelWithNoChain
    {
    }

    public class ModelWithoutNewUrl
    {
    }



    public class UrlModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class SubclassUrlModel : UrlModel
    {
    }

    public class ModelWithQueryStringInput
    {
        [QueryString]
        public int Param { get; set; }
    }

    public class ModelWithQueryStringAndRouteInput
    {
        [QueryString]
        public int Param { get; set; }

        public int RouteParam { get; set; }
    }
}
