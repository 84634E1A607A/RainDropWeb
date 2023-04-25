class EngineeringValue {
    static To(value) {
        const negative = value < 0;

        if (negative) value = -value;

        if (value < 1e-6) {
            return (negative ? "-" : "") + value * 1e9 + "n";
        }
        else if (value < 1e-3) {
            return (negative ? "-" : "") + value * 1e6 + "u";
        }
        else if (value < 1) {
            return (negative ? "-" : "") + value * 1e3 + "m";
        }
        else if (value < 1e3) {
            return (negative ? "-" : "") + value;
        }
        else if (value < 1e6) {
            return (negative ? "-" : "") + value / 1e3 + "k";
        }
        else if (value < 1e9) {
            return (negative ? "-" : "") + value / 1e6 + "M";
        }
        else {
            return (negative ? "-" : "") + value / 1e9 + "G";
        }
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
