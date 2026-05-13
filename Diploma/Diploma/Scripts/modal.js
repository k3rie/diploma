let modalCount = 0;

function openModal(url, data = null) {
    modalCount++;
    const modalId = `modal-window-${modalCount}`;
    const contentId = `modal-content-${modalCount}`;
    $('#loader').show();
    // HTML контейнера модалки
    const modalHTML = `
            <div id="${modalId}" class="modal-overlay" style="z-index:${1000 + modalCount}">
                <div class="modal">
                    <div class="modal-header">
                        <i data-lucide="x" class="modal-close" onclick="closeModal('${modalId}')"></i>
                    </div>
                    <div id="${contentId}" class="modal-body">
                        <div class="modal-loader">Loading...</div>
                    </div>
                </div>
            </div>
        `;

    document.body.insertAdjacentHTML('beforeend', modalHTML);
    lucide.createIcons();

    // AJAX загрузка содержимого
    $.ajax({
        url: url,
        type: "GET",
        data: data,
        traditional: true, // <<< вот эта строчка фиксит
        success: function (response) {
            $(`#${contentId}`).html(response);
            lucide.createIcons();
            $('#loader').hide();
        },
        error: function () {
            $(`#${contentId}`).html(`<p style="color:red">Load error!</p>`);
            $('#loader').hide();
        }
    });
}
function openModalNotUrl() {
    modalCount++;
    const modalId = `modal-window-${modalCount}`;
    const contentId = `modal-content-${modalCount}`;
    $('#loader').show();
    const modalHTML = `
          <div id="${modalId}" class="modal-overlay" style="z-index:${1000 + modalCount}">
              <div class="modal">
                  <div class="modal-header">
                      <i data-lucide="x" class="modal-close" onclick="closeModal('${modalId}')"></i>
                  </div>
                  <div id="${contentId}" class="modal-body">
                      <div class="modal-loader">Loading...</div>
                  </div>
              </div>
          </div>
      `;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    document.body.style.overflow = 'hidden';
    lucide.createIcons();

    return modalId; // возвращаем id модалки
}
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.remove();
    }
}

// Закрытие по Esc
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const modals = document.querySelectorAll('.modal-overlay');
        if (modals.length > 0) {
            closeModal(modals[modals.length - 1].id);
        }
    }
});

function modalOpen(modalId) {
    // Показываем модальное окно
    document.getElementById(modalId).style.display = 'flex';
}

function modalClose(modalId) {
    document.getElementById(modalId).style.display = 'none';
}
function closeLastModal() {
    const modals = document.querySelectorAll('.modal-overlay');
    if (modals.length > 0) {
        const lastModal = modals[modals.length - 1];
        lastModal.remove();
        modalCount = Math.max(0, modalCount - 1);
    }
}