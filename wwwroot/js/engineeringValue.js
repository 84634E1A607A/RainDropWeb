class EngineeringValue {
    static To(value) {
        if (value === 0)
            return "0";

        const negative = value < 0;
        let suffix = "";

        if (negative) value = -value;

        if (value < 1e-6) {
            value *= 1e9;
            suffix = "n";
        } else if (value < 1e-3) {
            value *= 1e6;
            suffix = "u";
        } else if (value < 1) {
            value *= 1e3;
            suffix = "m";
        } else if (value < 1e3) {
        } else if (value < 1e6) {
            value /= 1e3;
            suffix = "k";
        } else if (value < 1e9) {
            value /= 1e6;
            suffix = "M";
        } else {
            value /= 1e9;
            suffix = "G";
        }

        return (negative ? "-" : "") + value.toPrecision(3) + suffix;
    }

    static From(string) {
        let value = parseFloat(string);

        if (string.search(/[0-9.]m$/) !== -1) {
            return value * 1e-3;
        } else if (string.search(/[0-9.]u$/) !== -1) {
            return value * 1e-6;
        } else if (string.search(/[0-9.]n$/) !== -1) {
            return value * 1e-9;
        } else if (string.search(/[0-9.]k$/) !== -1) {
            return value * 1e3;
        } else if (string.search(/[0-9.]M$/) !== -1) {
            return value * 1e6;
        } else if (string.search(/[0-9.]G$/) !== -1) {
            return value * 1e9;
        }

        return value;
    }
}
