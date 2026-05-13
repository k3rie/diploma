document.addEventListener('DOMContentLoaded', function () {

    const periodBtn = document.getElementById('periodBtn');
    const presetDropdown = document.getElementById('presetDropdown');
    const startDateInput = document.getElementById('PeriodBegin');
    const endDateInput = document.getElementById('PeriodEnd');

    function formatDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    function setDateRange(startDate, endDate) {
        startDateInput.value = formatDate(startDate);
        endDateInput.value = formatDate(endDate);
    }

    const presetGenerators = {
        today: () => {
            const t = new Date();
            return [t, t];
        },
        yesterday: () => {
            const t = new Date();
            t.setDate(t.getDate() - 1);
            return [t, t];
        },
        thisWeek: () => {
            const t = new Date();
            const dow = t.getDay();
            const mon = new Date(t);
            mon.setDate(t.getDate() - (dow === 0 ? 6 : dow - 1));
            const sun = new Date(mon);
            sun.setDate(mon.getDate() + 6);
            return [mon, sun];
        },
        lastWeek: () => {
            const t = new Date();
            const dow = t.getDay();
            const lastMon = new Date(t);
            lastMon.setDate(t.getDate() - (dow === 0 ? 6 : dow - 1) - 7);
            const lastSun = new Date(lastMon);
            lastSun.setDate(lastMon.getDate() + 6);
            return [lastMon, lastSun];
        },
        thisMonth: () => {
            const t = new Date();
            const start = new Date(t.getFullYear(), t.getMonth(), 1);
            const end = new Date(t.getFullYear(), t.getMonth() + 1, 0);
            return [start, end];
        },
        lastMonth: () => {
            const t = new Date();
            return [
                new Date(t.getFullYear(), t.getMonth() - 1, 1),
                new Date(t.getFullYear(), t.getMonth(), 0)
            ];
        },
        thisQuarter: () => {
            const t = new Date();
            const q = Math.floor(t.getMonth() / 3);
            const start = new Date(t.getFullYear(), q * 3, 1);
            const end = new Date(t.getFullYear(), q * 3 + 3, 0);
            return [start, end];
        },
        lastQuarter: () => {
            const t = new Date();
            let q = Math.floor(t.getMonth() / 3) - 1;
            let y = t.getFullYear();
            let m = q * 3;

            if (q < 0) {
                y = y - 1;
                q = 3;
                m = 9;
            }

            return [
                new Date(y, m, 1),
                new Date(y, m + 3, 0)
            ];
        },
        thisYear: () => {
            const t = new Date();
            return [
                new Date(t.getFullYear(), 0, 1),
                new Date(t.getFullYear(), 11, 31)
            ];
        },
        thisYearToToday: () => {
            const t = new Date();
            const start = new Date(t.getFullYear(), 0, 1);
            const end = t;
            return [start, end];
        },
        lastYear: () => {
            const t = new Date();
            const y = t.getFullYear() - 1;
            return [new Date(y, 0, 1), new Date(y, 11, 31)];
        }
    };

    function matchPreset(start, end) {
        if (!start || !end) return null;

        const norm = d => formatDate(new Date(d));
        const ns = norm(start);
        const ne = norm(end);

        for (const type in presetGenerators) {
            const [ps, pe] = presetGenerators[type]();
            if (norm(ps) === ns && norm(pe) === ne) {
                return type;
            }
        }
        return null;
    }

    function updateButtonText() {
        const s = startDateInput.value;
        const e = endDateInput.value;

        if (!s || !e) return;

        const preset = matchPreset(s, e);

        if (preset) {
            const btn = presetDropdown.querySelector(`[data-type="${preset}"]`);
            if (btn) {
                periodBtn.textContent = btn.textContent;
                return;
            }
        }

        periodBtn.textContent = 'Custom Period';
    }

    // INIT
    updateButtonText();

    // toggle dropdown
    periodBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        presetDropdown.style.display =
            presetDropdown.style.display === 'flex' ? 'none' : 'flex';
    });

    document.addEventListener('click', function (e) {
        if (!presetDropdown.contains(e.target) && e.target !== periodBtn) {
            presetDropdown.style.display = 'none';
        }
    });

    presetDropdown.querySelectorAll('button').forEach(btn => {
        btn.addEventListener('click', function () {
            const type = this.dataset.type;
            const [start, end] = presetGenerators[type]();

            setDateRange(start, end);
            periodBtn.textContent = this.textContent;
            presetDropdown.style.display = 'none';
        });
    });

    startDateInput.addEventListener('change', updateButtonText);
    endDateInput.addEventListener('change', updateButtonText);

});