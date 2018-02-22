/* eslint-disable no-unused-vars, no-undef */
var profile = {
    resourceTags: {
        test(mid) {
            return /\/Spec/.test(mid);
        },
        copyOnly(filename, mid) {
            return (/^app\/resources\//.test(mid) && !/\.css$/.test(filename));
        },
        amd(filename, mid) {
            return !this.copyOnly(filename, mid) && /\.js$/.test(filename);
        },
        miniExclude(filename, mid) {
            return mid in {
                'app/package': 1,
                'app/tests/jasmineTestBootstrap': 1
            };
        }
    }
};
